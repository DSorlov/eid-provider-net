using System.Security.Cryptography.X509Certificates;
using com.sorlov.eidprovider;

namespace com.sorlov.eidprovider.bankid
{
    public class InitializationData: IInitializationData
    {
        public string Endpoint;
        public bool AllowFingerprint;
        public X509Certificate2 CACertificate;
        public X509Certificate2 ClientCertificate;

        public InitializationData(Enviroments enviroment)
        {
            if (enviroment == Enviroments.production)
            {
                Endpoint = "https://appapi2.bankid.com/rp/v5";
                AllowFingerprint = true;
                CACertificate = new X509Certificate2(Helpers.GetCertFile("bankid_prod.ca"));
            }
            else
            {
                Endpoint = "https://appapi2.test.bankid.com/rp/v5";
                AllowFingerprint = true;
                CACertificate = new X509Certificate2(Helpers.GetCertFile("bankid_test.ca"));
                ClientCertificate = new X509Certificate2(Helpers.GetCertFile("bankid_test.pfx"), "qwerty123");
            }
        }
    } 


}