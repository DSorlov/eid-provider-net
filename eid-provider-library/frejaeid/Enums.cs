using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.frejaeid
{
    public enum LOALevel
    {
        BASIC,
        EXTENDED,
        PLUS
    }

    public enum UserInfo
    {
        PHONE,
        EMAIL,
        SSN,
        INFERRED,
        ORG_ID
    }

    [Flags]
    public enum Attributes
    {
        NONE = 0,
        BASIC_USER_INFO = 1,
        EMAIL_ADDRESS = 2,
        ALL_EMAIL_ADDRESSES = 4,
        DATE_OF_BIRTH = 8,
        ADDRESSES = 16,
        SSN = 32,
        RELYING_PARTY_USER_ID = 64,
        INTEGRATOR_SPECIFIC_USER_UD = 128,
        CUSTOM_IDENTIFIER = 256,
        ORG_ID = 512,
        PHOTO = 1024,
        AGE = 2048,
        DOCUMENT = 4096,
        COVID_CERTIFICATES = 8192
    }

    public enum SSNCountry
    {
        SE,
        DK,
        NO,
        FI
    }
}
