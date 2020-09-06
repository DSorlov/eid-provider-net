using System;
using System.Collections.Generic;
using System.Text;

namespace com.sorlov.eidprovider
{
    public class ApiCompletedResponse : ApiResponse
    {
        public ApiCompletedResponseUser User;
        public Dictionary<string, string> Extra;

        public ApiCompletedResponse(ApiCompletedResponseUser user)
        {
            Extra = new Dictionary<string, string>();
            User = user;
            base.Status = ResponseType.completed;
        }
    }

    public class ApiCompletedResponseUser
    {
        public string Id;
        public string Firstname;
        public string Surname;
        public string Fullname;

        public ApiCompletedResponseUser(string id, string firstname, string surname, string fullname)
        {
            Id = id;
            Firstname = firstname;
            Surname = surname;
            Fullname = fullname;
        }
    }
}
