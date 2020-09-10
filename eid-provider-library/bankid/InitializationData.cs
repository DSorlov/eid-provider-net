using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.bankid
{
    public class InitializationData : EIDClientInitializationData
    {

        public InitializationData(EIDEnvironment environment) : base(environment)
        {
            if (environment == EIDEnvironment.Production)
            {
                this.Add("endpoint", "https://appapi2.bankid.com/rp/v5");
                this.Add("allowFingerprint", "true");
                this.Add("ca_cert", "builtin://certs/bankid_prod.ca");
                this.Add("client_cert", null);
                this.Add("password", "");
            }
            else
            {
                this.Add("endpoint", "https://appapi2.test.bankid.com/rp/v5");
                this.Add("allowFingerprint", "true");
                this.Add("ca_cert", "builtin://certs/bankid_test.ca");
                this.Add("client_cert", "builtin://certs/bankid_test.pfx");
                this.Add("password", "qwerty123");
            }

        }
    }
}
