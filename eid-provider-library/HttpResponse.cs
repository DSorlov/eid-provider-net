using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    public class HttpResponse
    {
        public int HttpStatusCode {
            get => httpStatusCode;
        }
        private int httpStatusCode;
        public string HttpStatusMessage
        {
            get => httpStatusMessage;
        }
        private string httpStatusMessage;
        public Dictionary<string,string> Headers
        {
            get => headers;
        }
        private Dictionary<string,string> headers;
        public JObject Data
        {
            get => data;
        }
        private JObject data;
        public string RawData
        {
            get => rawData;
        }
        private string rawData;

        public JToken this[string i]
        {
            get
            {
                try
                {
                    return (JToken)data[i];
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool ContainsKey(string s)
        {
            return data.ContainsKey(s);
        }

        internal HttpResponse(int httpStatusCode, string httpStatusMessage, Dictionary<string,string> headers, JObject data, string rawData)
        {
            this.httpStatusCode = httpStatusCode;
            this.httpStatusMessage = httpStatusMessage;
            this.headers = headers;
            this.rawData = rawData;
            this.data = data;
        }
    }
}
