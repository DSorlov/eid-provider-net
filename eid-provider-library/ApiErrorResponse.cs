using System;
using System.Collections.Generic;
using System.Text;

namespace com.sorlov.eidprovider
{
    public class ApiErrorResponse: ApiResponse
    {
        public enum ErrorCode
        {
            system_error,
            api_error,
            already_in_progress,
            request_text_invalid,
            request_ssn_invalid,
            cancelled_by_user,
            cancelled_by_idp,
            expired_transaction,
            request_id_invalid
        }

        public ErrorCode Code;
        public string Description;
        public string Details;

        public ApiErrorResponse(ErrorCode code, string description, string details = "")
        {
            Code = code;
            Description = description;
            Details = details;
            base.Status = ResponseType.error;

        }

    }
}
