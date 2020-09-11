using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace com.sorlov.eidprovider.frejaeid
{

    public class JSonWebToken
    {
        private JObject header;
        public JObject Header
        {
            get => header;
        }
        private JObject payload;
        public JObject Payload
        {
            get => payload;
        }
        private bool isValid;
        public bool IsValid
        {
            get => isValid;
        }

        protected JSonWebToken(JObject header, JObject payload, bool isValid)
        {
            this.header = header;
            this.payload = payload;
            this.isValid = isValid;
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 1: output += "==="; break; // Three pad chars
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }

        public static JSonWebToken FromString(string token, Dictionary<string, X509Certificate2> keys)
        {
            string[] parts = token.Split('.');
            string header = parts[0];
            string payload = parts[1];
            byte[] crypto = Base64UrlDecode(parts[2]);

            string headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
            JObject headerData = JObject.Parse(headerJson);

            string payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            JObject payloadData = JObject.Parse(payloadJson);

            string pPublicKey = BuildPublicKeyPem(keys[(string)headerData["x5t"]]);
            pPublicKey = Regex.Replace(pPublicKey, @"\r\n?|\n", "");
            pPublicKey = pPublicKey.Replace("-----BEGIN PUBLIC KEY-----", "");
            pPublicKey = pPublicKey.Replace("-----END PUBLIC KEY-----", "");
            byte[] lDer = Convert.FromBase64String(pPublicKey);

            string exponentString = Encoding.ASCII.GetString(GetExponent(lDer));

            RSAParameters rsaParameters = new RSAParameters();
            rsaParameters.Modulus = GetModulus(lDer);
            rsaParameters.Exponent = GetExponent(lDer);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParameters);

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(parts[0] + "." + parts[1]));

            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");


            return new JSonWebToken(headerData, payloadData, !rsaDeformatter.VerifySignature(hash, crypto));
        }

        private static byte[] GetModulus(byte[] pDer)
        {
            //Size header is 29 bits
            //The key size modulus is 128 bits, but in hexa string the size is 2 digits => 256 
            string lModulus = BitConverter.ToString(pDer).Replace("-", "").Substring(58, 256);

            return StringHexToByteArray(lModulus);
        }

        private static byte[] GetExponent(byte[] pDer)
        {
            int lExponentLenght = pDer[pDer.Length - 3];
            string lExponent = BitConverter.ToString(pDer).Replace("-", "").Substring((pDer.Length * 2) - lExponentLenght * 2, lExponentLenght * 2);

            return StringHexToByteArray(lExponent);
        }

        public static byte[] StringHexToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string BuildPublicKeyPem(X509Certificate2 cert)
        {
            byte[] algOid;

            switch (cert.GetKeyAlgorithm())
            {
                case "1.2.840.113549.1.1.1":
                    algOid = new byte[] { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01 };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cert), $"Need an OID lookup for {cert.GetKeyAlgorithm()}");
            }

            byte[] algParams = cert.GetKeyAlgorithmParameters();
            byte[] publicKey = WrapAsBitString(cert.GetPublicKey());

            byte[] algId = BuildSimpleDerSequence(algOid, algParams);
            byte[] spki = BuildSimpleDerSequence(algId, publicKey);

            return PemEncode(spki, "PUBLIC KEY");
        }

        private static string PemEncode(byte[] berData, string pemLabel)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("-----BEGIN ");
            builder.Append(pemLabel);
            builder.AppendLine("-----");
            builder.AppendLine(Convert.ToBase64String(berData, Base64FormattingOptions.InsertLineBreaks));
            builder.Append("-----END ");
            builder.Append(pemLabel);
            builder.AppendLine("-----");

            return builder.ToString();
        }

        private static byte[] BuildSimpleDerSequence(params byte[][] values)
        {
            int totalLength = values.Sum(v => v.Length);
            byte[] len = EncodeDerLength(totalLength);
            int offset = 1;

            byte[] seq = new byte[totalLength + len.Length + 1];
            seq[0] = 0x30;

            Buffer.BlockCopy(len, 0, seq, offset, len.Length);
            offset += len.Length;

            foreach (byte[] value in values)
            {
                Buffer.BlockCopy(value, 0, seq, offset, value.Length);
                offset += value.Length;
            }

            return seq;
        }

        private static byte[] WrapAsBitString(byte[] value)
        {
            byte[] len = EncodeDerLength(value.Length + 1);
            byte[] bitString = new byte[value.Length + len.Length + 2];
            bitString[0] = 0x03;
            Buffer.BlockCopy(len, 0, bitString, 1, len.Length);
            bitString[len.Length + 1] = 0x00;
            Buffer.BlockCopy(value, 0, bitString, len.Length + 2, value.Length);
            return bitString;
        }

        private static byte[] EncodeDerLength(int length)
        {
            if (length <= 0x7F)
            {
                return new byte[] { (byte)length };
            }

            if (length <= 0xFF)
            {
                return new byte[] { 0x81, (byte)length };
            }

            if (length <= 0xFFFF)
            {
                return new byte[]
                {
            0x82,
            (byte)(length >> 8),
            (byte)length,
                };
            }

            if (length <= 0xFFFFFF)
            {
                return new byte[]
                {
            0x83,
            (byte)(length >> 16),
            (byte)(length >> 8),
            (byte)length,
                };
            }

            return new byte[]
            {
        0x84,
        (byte)(length >> 24),
        (byte)(length >> 16),
        (byte)(length >> 8),
        (byte)length,
            };
        }

    }
}