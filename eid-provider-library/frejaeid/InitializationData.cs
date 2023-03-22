using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.frejaeid
{
    public class InitializationData : EIDClientInitializationData
    {

        public InitializationData(EIDEnvironment environment) : base(environment)
        {
            if (environment == EIDEnvironment.Production)
            {
                this.Add("endpoint", "https://services.prod.frejaeid.com");
                this.Add("ca_cert", "builtin://certs/frejaeid_prod.ca");
                this.Add("client_cert", null);
                this.Add("password", "");
                this.Add("id_type", "SSN");
                this.Add("attribute_list", "EMAIL_ADDRESS,RELYING_PARTY_USER_ID,BASIC_USER_INFO");
                this.Add("minimum_level", "EXTENDED");
                this.Add("default_country", "SE");
                this.Add("jwt_cert", "{'aRw9OLn2BhM7hxoc458cIXHfezw': 'builtin://certs/frejaeid_prod_aRw9OLn2BhM7hxoc458cIXHfezw.jwt', 'wSYLdhe93ToPR2X1UrNXxOg1juI': 'builtin://certs/frejaeid_prod_wSYLdhe93ToPR2X1UrNXxOg1juI.jwt'}");
            }
            else
            {
                this.Add("endpoint", "https://services.test.frejaeid.com");
                this.Add("ca_cert", "builtin://certs/frejaeid_test.ca");
                this.Add("client_cert", "builtin://certs/frejaeid_test.pfx");
                this.Add("password", "test");
                this.Add("id_type", "SSN");
                this.Add("attribute_list", "EMAIL_ADDRESS,RELYING_PARTY_USER_ID,BASIC_USER_INFO");
                this.Add("minimum_level", "EXTENDED");
                this.Add("default_country", "SE");
                this.Add("jwt_cert", "{'2LQIrINOzwWAVDhoYybqUcXXmVs': 'builtin://certs/frejaeid_test_2LQIrINOzwWAVDhoYybqUcXXmVs.jwt', 'DiZbzBfysUm6-IwI-GtienEsbjc': 'builtin://certs/frejaeid_test_DiZbzBfysUm6-IwI-GtienEsbjc.jwt'}");
            }

        }
    }
}
