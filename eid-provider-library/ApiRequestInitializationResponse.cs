using System;
using System.Collections.Generic;
using System.Text;

namespace com.sorlov.eidprovider
{
    public class ApiRequestInitializationResponse : ApiResponse
    {
        public string Id;
        public Dictionary<string, string> Extra;

        public ApiRequestInitializationResponse(string id)
        {
            Id = id;
            Extra = new Dictionary<string, string>();
            base.Status = ResponseType.initialized;
        }

    }
}
