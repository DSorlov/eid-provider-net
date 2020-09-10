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
                this.Add("jwt_cert", "{'aRw9OLn2BhM7hxoc458cIXHfezw': 'builtin://certs/frejaeid_prod_aRw9OLn2BhM7hxoc458cIXHfezw.jwt', 'onjnxVgI3oUzWQMLciD7sQZ4mqM': 'builtin://certs/frejaeid_prod_onjnxVgI3oUzWQMLciD7sQZ4mqM.jwt'}");
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
                this.Add("jwt_cert", "{'2LQIrINOzwWAVDhoYybqUcXXmVs': 'builtin://certs/frejaeid_test_2LQIrINOzwWAVDhoYybqUcXXmVs.jwt', 'HwMHK_gb3_iuNF1advMtlG0-fUs': 'builtin://certs/frejaeid_test_HwMHK_gb3_iuNF1advMtlG0-fUs.jwt'}");
            }

        }
    }
}
