using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.bankid
{
    public class Client: IClient
    {

        private X509Certificate2 cACert;
        private X509Certificate2 clientCert;
        private bool allowFingerprint;
        private string httpEndpoint;

        public Client(IInitializationData settings)
        {
            if (settings.GetType() != typeof(InitializationData))
                throw new NotSupportedException("Can only process BankID Initialization data");

            clientCert = ((InitializationData)settings).ClientCertificate;
            cACert = ((InitializationData)settings).CACertificate;
            allowFingerprint= ((InitializationData)settings).AllowFingerprint;
            httpEndpoint = ((InitializationData)settings).Endpoint;
        }

        public event EventHandler StatusUpdate;
        protected virtual void OnStatusUpdate(ApiEventArgs e)
        {
            StatusUpdate?.Invoke(this, e);
        }

        public ApiResponse AuthRequest(string id)
        {
            ApiResponse apiResponse = InitAuthRequest(id);
            if (apiResponse.Status != ApiResponse.ResponseType.initialized) return apiResponse;

            OnStatusUpdate(new ApiEventArgs(apiResponse));
            while (true)
            {
                ApiResponse pollResponse = PollAuthRequest(((ApiRequestInitializationResponse)apiResponse).Id);

                if (pollResponse.Status == ApiResponse.ResponseType.error)
                    return pollResponse;

                if (pollResponse.Status == ApiResponse.ResponseType.completed)
                    return pollResponse;

                if (pollResponse.Status == ApiResponse.ResponseType.pending)
                    OnStatusUpdate(new ApiEventArgs(pollResponse));

                Thread.Sleep(2000);
            }

        }

        public ApiResponse SignRequest(string id, string agreementText)
        {
            ApiResponse apiResponse = InitSignRequest(id, agreementText);
            if (apiResponse.Status != ApiResponse.ResponseType.initialized) return apiResponse;

            OnStatusUpdate(new ApiEventArgs(apiResponse));
            while (true)
            {
                ApiResponse pollResponse = PollSignRequest(((ApiRequestInitializationResponse)apiResponse).Id);

                if (pollResponse.Status == ApiResponse.ResponseType.error)
                    return pollResponse;

                if (pollResponse.Status == ApiResponse.ResponseType.completed)
                    return pollResponse;

                if (pollResponse.Status == ApiResponse.ResponseType.pending)
                    OnStatusUpdate(new ApiEventArgs(pollResponse));

                Thread.Sleep(2000);
            }

        }

        public ApiResponse cancelRequest(string requestId)
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; ///TODO: Fix remote validation, now swallow all

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpEndpoint + "/cancel");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ClientCertificates.Add(clientCert);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                streamWriter.Write("{\"orderRef\":\"" + requestId + "\"}");

            return new ApiResponse() { Status = ApiResponse.ResponseType.cancelled };
        }

        public ApiResponse CancelAuthRequest(string requestId)
        {
            return cancelRequest(requestId);
        }

        public ApiResponse CancelSignRequest(string requestId)
        {
            return cancelRequest(requestId);
        }

        private ApiResponse initRequest(string jsonData,string endpoint) {

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; ///TODO: Fix remote validation, now swallow all

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpEndpoint+"/"+endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ClientCertificates.Add(clientCert);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
            }

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string postResults = streamReader.ReadToEnd();
                    Dictionary<string, string> responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(postResults);

                    if (responseObject.ContainsKey("orderRef"))
                    {
                        ApiRequestInitializationResponse response = new ApiRequestInitializationResponse(responseObject["orderRef"]);
                        response.Extra.Add("autostart_token", responseObject["autoStartToken"]);
                        response.Extra.Add("autostart_url", "bankid:///?autostarttoken=" + responseObject["autoStartToken"] + "&redirect=null");
                        return response;
                    }

                }
            }
            catch (WebException e)
            {
                var response = ((HttpWebResponse)e.Response);
                int code = (int)((HttpWebResponse)e.Response).StatusCode;

                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string postResults = streamReader.ReadToEnd();
                    Dictionary<string, string> responseObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(postResults);

                    if (responseObject.ContainsKey("errorCode"))
                    {
                        switch (responseObject["errorCode"].ToString())
                        {
                            case "alreadyInProgress":
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.already_in_progress, "A transaction was already pending for this SSN"); ;
                            case "invalidParameters":

                                switch (responseObject["details"].ToString())
                                {
                                    case "Incorrect personalNumber":
                                        return new ApiErrorResponse(ApiErrorResponse.ErrorCode.request_ssn_invalid, "The supplied SSN is not valid"); ;
                                    case "Invalid userVisibleData":
                                        return new ApiErrorResponse(ApiErrorResponse.ErrorCode.request_text_invalid, "The supplied agreement text is not valid"); ;
                                    default:
                                        return new ApiErrorResponse(ApiErrorResponse.ErrorCode.api_error, "A communications error occured", responseObject["details"].ToString());
                                }

                            default:
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.api_error, "A communications error occured", responseObject["errorCode"].ToString());
                        }

                    }
                    else
                    {
                        return new ApiErrorResponse(ApiErrorResponse.ErrorCode.system_error, "Unkown response from remote API"); ;
                    }
                }
            }

            return new ApiErrorResponse(ApiErrorResponse.ErrorCode.system_error, "Unkown response from remote API"); ;
        }

        public ApiResponse InitAuthRequest(string id)
        {
            string jsonData = "{" +
                "\"endUserIp\":\"127.0.0.1\"," +
                "\"personalNumber\":\""+id+"\","+
                "\"requirement\": {" +
                    "\"allowFingerprint\":" + allowFingerprint.ToString().ToLower() + ""+
                    "}"+
                "}";

            return initRequest(jsonData,"auth");
        }
        public ApiResponse InitSignRequest(string id, string agreementText)
        {
            string jsonData = "{" +
                "\"endUserIp\":\"127.0.0.1\"," +
                "\"personalNumber\":\""+id+"\","+
                "\"userVisibleData\":\""+System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(agreementText))+"\","+
                "\"requirement\": {" +
                    "\"allowFingerprint\": " + allowFingerprint.ToString().ToLower()+""+
                    "}"+
                "}";

            return initRequest(jsonData,"sign");
        }

        public ApiResponse pollRequest(string requestId)
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true; ///TODO: Fix remote validation, now swallow all

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpEndpoint + "/collect");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ClientCertificates.Add(clientCert);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                streamWriter.Write("{\"orderRef\":\"" + requestId + "\"}");

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string postResults = streamReader.ReadToEnd();
                    JObject responseObject = JsonConvert.DeserializeObject<JObject>(postResults);

                    //string hintCode = Helpers.GetDynamic<string>(responseObject, "hintCode");
                    if (responseObject.ContainsKey("hintCode"))
                    {
                        switch (responseObject["hintCode"].ToString())
                        {
                            case "expiredTransaction":
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.expired_transaction, "The transaction was not completed in time"); ;
                            case "outstandingTransaction":
                                return new ApiPendingResponse(ApiPendingResponse.PendingCode.pending_notdelivered, "The transaction has not initialized yet");
                            case "userSign":
                                return new ApiPendingResponse(ApiPendingResponse.PendingCode.pending_user_in_app, "User have started the app");
                            case "noClient":
                                return new ApiPendingResponse(ApiPendingResponse.PendingCode.pending_delivered, "Delivered to mobile phone");
                            case "userCancel":
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.cancelled_by_user, "The user declined transaction"); ;
                            case "cancelled":
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.cancelled_by_idp, "The IdP have cancelled the request"); ;
                            default:
                                return new ApiErrorResponse(ApiErrorResponse.ErrorCode.api_error, "A communications error occured", responseObject["hintCode"].ToString());
                        }

                    }

                    ApiCompletedResponseUser responseUser = new ApiCompletedResponseUser(responseObject["completionData"]["user"]["personalNumber"].ToString(), responseObject["completionData"]["user"]["givenName"].ToString(), responseObject["completionData"]["user"]["surname"].ToString(), responseObject["completionData"]["user"]["name"].ToString());
                    ApiCompletedResponse response = new ApiCompletedResponse(responseUser);
                    response.Extra.Add("signature", responseObject["completionData"]["signature"].ToString());
                    response.Extra.Add("ocspResponse", responseObject["completionData"]["ocspResponse"].ToString());
                    return response;
                }
            }
            catch (WebException e)
            {
                var response = ((HttpWebResponse)e.Response);
                int code = (int)((HttpWebResponse)e.Response).StatusCode;

                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string postResults = streamReader.ReadToEnd();

                    try
                    {
                        JObject responseObject = JsonConvert.DeserializeObject<JObject>(postResults);
                        if (responseObject.ContainsKey("errorCode"))
                        {
                            switch (responseObject["errorCode"].ToString())
                            {
                                case "invalidParameters":
                                    return new ApiErrorResponse(ApiErrorResponse.ErrorCode.request_id_invalid, "The supplied request cannot be found"); ;
                                default:
                                    return new ApiErrorResponse(ApiErrorResponse.ErrorCode.api_error, "A communications error occured", responseObject["hintCode"].ToString());
                            }

                        }
                        else
                            return new ApiErrorResponse(ApiErrorResponse.ErrorCode.api_error, "Unkown response from remote API", postResults);

                    }
                    catch
                    {
                        return new ApiErrorResponse(ApiErrorResponse.ErrorCode.system_error, "The remote server response could not be deserialized");
                    }

                }
            }

        }

        public ApiResponse PollAuthRequest(string requestId)
        {
            return pollRequest(requestId);
        }

        public ApiResponse PollSignRequest(string requestId)
        {
            return pollRequest(requestId);
        }
    }

}
