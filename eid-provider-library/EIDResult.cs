using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace com.sorlov.eidprovider
{
    public class EIDResult
    {
        public enum ResultStatus
        {
            error,
            initialized,
            completed,
            pending,
            cancelled,
            ok
        }

        public ResultStatus Status
        {
            get => status;
        }
        private ResultStatus status;

        public JToken this[string i]
        {
            get
            {
                try
                {
                    return data[i];
                }
                catch
                {
                    return null;
                }
            }
        }
        private JObject data;

        public override string ToString()
        {
            JObject dataCopy = data;
            dataCopy["Status"] = status.ToString();

            return dataCopy.ToString();
        }

        internal static EIDResult CreateCancelledResult()
        {
            JObject data = new JObject();
            return new EIDResult(ResultStatus.cancelled, data);
        }

        internal static EIDResult CreateErrorResult(string code, string description, object details)
        {
            JObject data = new JObject();
            data["code"] = code;
            data["description"] = description;
            data["details"] = JsonConvert.SerializeObject(details);
            return new EIDResult(ResultStatus.error, data);
        }

        internal static EIDResult CreateErrorResult(string code, string description)
        {
            JObject data = new JObject();
            data["code"] = code;
            data["description"] = description;
            return new EIDResult(ResultStatus.error, data);
        }

        internal static EIDResult CreateOKResult(string code, string description)
        {
            JObject data = new JObject();
            data["code"] = code;
            data["description"] = description;
            return new EIDResult(ResultStatus.ok, data);
        }
        internal static EIDResult CreateOKResult(string code, string description, JObject extra)
        {
            JObject data = new JObject();
            data["code"] = code;
            data["description"] = description;
            data["extra"] = extra;
            return new EIDResult(ResultStatus.ok, data);
        }

        internal static EIDResult CreatePendingResult(string code, string description)
        {
            JObject data = new JObject();
            data["code"] = code;
            data["description"] = description;
            return new EIDResult(ResultStatus.pending, data);
        }

        internal static EIDResult CreateCompletedResult(JObject data)
        {
            return new EIDResult(ResultStatus.completed, data);
        }
        internal static EIDResult CreateInitializedResult(JObject data)
        {
            return new EIDResult(ResultStatus.initialized, data);
        }

        internal EIDResult(ResultStatus result, JObject data)
        {
            this.status = result;
            this.data = data;
        }
    }
}
