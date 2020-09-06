using System;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using com.sorlov.eidprovider.bankid;

namespace com.sorlov.eidprovider
{
    public static class Helpers
    {

        public static byte[] GetCertFile(string fileName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("com.sorlov.eidprovider.certs." + fileName))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

        }

    }
}