using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.bankid
{
    public class Client : EIDClient
    {
        private X509Certificate2 caCertificate;
        private X509Certificate2 clientCertificate;
        private bool allowFingerprint;
        private string httpEndpoint;

        public Client(EIDClientInitializationData configuration) : base(configuration) {

            try
            {
                caCertificate = new X509Certificate2(EIDResourceManager.ResolveResource(configuration["ca_cert"]));
                clientCertificate = new X509Certificate2(EIDResourceManager.ResolveResource(configuration["client_cert"]), configuration["password"]);
                allowFingerprint = configuration["allowFingerprint"] == null ? false : Boolean.Parse(configuration["allowFingerprint"]);
                httpEndpoint = configuration["endpoint"];
            }
            catch
            {
                throw new ArgumentException("Configuration block was not valid");
            }
            
        }

        public override EIDResult CancelAuthRequest(string id)
        {
            return cancelRequest(id);
        }

        public override EIDResult CancelSignRequest(string id)
        {
            return cancelRequest(id);
        }

        public override EIDResult InitAuthRequest(string id)
        {
            JObject postData = new JObject();
            postData["personalNumber"] = id;
            postData["requirement"] = new JObject();
            postData["requirement"]["allowFingerprint"] = allowFingerprint;
            postData["endUserIp"] = "127.0.0.1";

            return initRequest("auth", postData);
        }
        public override EIDResult InitSignRequest(string id, string text)
        {
            if (String.IsNullOrEmpty(text))
                return EIDResult.CreateErrorResult("request_text_invalid", "The supplied agreement text is not valid");

            JObject postData = new JObject();
            postData["personalNumber"] = id;
            postData["requirement"] = new JObject();
            postData["requirement"]["allowFingerprint"] = allowFingerprint;
            postData["endUserIp"] = "127.0.0.1";
            postData["userVisibleData"] = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(text));

            return initRequest("sign",postData);
        }

        public override EIDResult PollAuthRequest(string id)
        {
            return pollRequest(id);
        }

        public override EIDResult PollSignRequest(string id)
        {
            return pollRequest(id);
        }

        private EIDResult pollRequest(string id)
        {
            JObject postData = new JObject();
            postData["orderRef"] = id;
            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/collect", postData).Result;

            if (httpResponse.ContainsKey("hintCode"))
            {
                    switch (httpResponse["hintCode"].ToString())
                    {
                        case "expiredTransaction":
                            return EIDResult.CreateErrorResult("expired_transaction", "The transaction was not completed in time");
                        case "outstandingTransaction":
                            return EIDResult.CreatePendingResult("pending_notdelivered", "The transaction has not initialized yet");
                        case "userSign":
                            return EIDResult.CreatePendingResult("pending_user_in_app", "User have started the app");
                        case "noClient":
                            return EIDResult.CreatePendingResult("pending_delivered", "Delivered to mobile phone");
                        case "userCancel":
                            return EIDResult.CreateErrorResult("cancelled_by_user", "The user declined transaction");
                        case "cancelled":
                            return EIDResult.CreateErrorResult("cancelled_by_idp", "The IdP have cancelled the request");
                        default:
                            return EIDResult.CreateErrorResult("api_error", httpResponse["hintCode"].ToString());
                    }
            }

            if (httpResponse.ContainsKey("completionData"))
            {
                JObject result = new JObject();

                result["user"] = new JObject();
                result["user"]["id"] = httpResponse["completionData"]["user"]["personalNumber"].ToString();
                result["user"]["firstname"] = httpResponse["completionData"]["user"]["givenName"].ToString();
                result["user"]["lastname"] = httpResponse["completionData"]["user"]["surname"].ToString();
                result["user"]["fullname"] = httpResponse["completionData"]["user"]["name"].ToString();

                result["extra"] = new JObject();
                result["extra"]["signature"] = httpResponse["completionData"]["signature"].ToString();
                result["extra"]["ocspResponse"] = httpResponse["completionData"]["ocspResponse"].ToString();

                return EIDResult.CreateCompletedResult(result);

            }

            if (httpResponse.ContainsKey("errorCode"))
            {
                switch (httpResponse["errorCode"].ToString())
                {
                    case "invalidParameters":
                        return EIDResult.CreateErrorResult("request_id_invalid", "The supplied request cannot be found");
                    default:
                        return EIDResult.CreateErrorResult("api_error", httpResponse["errorCode"].ToString());
                }

            }

            return EIDResult.CreateErrorResult("system_error", "A communications error occured", httpResponse.HttpStatusMessage);
        }

        private EIDResult cancelRequest(string id)
        {
            JObject postData = new JObject();
            postData["orderRef"] = id;
            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/cancel", postData).Result;
            return EIDResult.CreateCancelledResult();
        }

        private EIDResult initRequest(string endpoint, JObject postData)
        {
            // Make the request
            HttpRequest httpRequest = new HttpRequest(caCertificate,clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/"+ endpoint, postData).Result;

            JObject result = new JObject();
            if (httpResponse.HttpStatusCode == 200)
            {

                if (httpResponse.ContainsKey("orderRef"))
                {
                    result["id"] = httpResponse["orderRef"].ToString();
                    result["extra"] = new JObject();
                    result["extra"]["autostart_token"] = httpResponse["autoStartToken"].ToString();
                    result["extra"]["autostart_url"] = "bankid:///?autostarttoken=" + result["extra"]["autostart_token"] + "&redirect=null";
                    return EIDResult.CreateInitializedResult(result);

                }

                return EIDResult.CreateErrorResult("api_error", "A communications error occured");

            }
            else {

                if (httpResponse.ContainsKey("errorCode"))
                {
                    switch (httpResponse["errorCode"].ToString())
                    {
                        case "alreadyInProgress":
                            return EIDResult.CreateErrorResult("already_in_progress", "A transaction was already pending for this SSN");
                        case "invalidParameters":

                            switch (httpResponse["details"].ToString())
                            {
                                case "Incorrect personalNumber":
                                    return EIDResult.CreateErrorResult("request_ssn_invalid", "The supplied SSN is not valid");
                                case "Invalid userVisibleData":
                                    return EIDResult.CreateErrorResult("request_text_invalid", "The supplied agreement text is not valid");
                                default:
                                    return EIDResult.CreateErrorResult("api_error", "A communications error occured", httpResponse["details"].ToString());
                            }

                        default:
                            return EIDResult.CreateErrorResult("api_error", "A communications error occured", httpResponse["errorCode"].ToString());
                    }

                }

                return EIDResult.CreateErrorResult("system_error", "A communications error occured", httpResponse.HttpStatusMessage);

            }
        }
    }
}
