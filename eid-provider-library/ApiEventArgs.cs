using System;
using System.Collections.Generic;
using System.Text;

namespace com.sorlov.eidprovider
{
    public class ApiEventArgs: EventArgs
    {
        public ApiResponse ApiResponse;

        public ApiEventArgs(ApiResponse apiResponse)
        {
            ApiResponse = apiResponse;
        }
    }
}
