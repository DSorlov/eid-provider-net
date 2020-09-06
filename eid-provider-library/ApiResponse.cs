using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace com.sorlov.eidprovider
{  
    public class ApiResponse
    {
        public enum ResponseType
        {
            initialized,
            pending,
            completed,
            cancelled,
            error
        }

        public ResponseType Status;

    }
}
