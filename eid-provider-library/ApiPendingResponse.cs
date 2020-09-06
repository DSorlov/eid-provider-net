using System;
using System.Collections.Generic;
using System.Text;

namespace com.sorlov.eidprovider
{
    public class ApiPendingResponse: ApiResponse
    {
        public enum PendingCode
        {
            pending_notdelivered,
            pending_user_in_app,
            pending_delivered
        }

        public PendingCode Code;
        public string Description;

        public ApiPendingResponse(PendingCode code, string description)
        {
            Code = code;
            Description = description;
            base.Status = ResponseType.pending;
        }

    }
}
