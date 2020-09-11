using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider.frejaeid
{
    public class Client : EIDClient
    {
        private X509Certificate2 caCertificate;
        private X509Certificate2 clientCertificate;
        private string httpEndpoint;
        private LOALevel minimumLevel;
        private SSNCountry defaultCountry;
        private UserInfo idType;
        private Attributes attributeList;
        private Dictionary<string, X509Certificate2> jwtCerts;

        public Client(EIDClientInitializationData configuration) : base(configuration) {

            try
            {
                //Deserialize the json and load all jwts - format compatible with eid-provider
                Dictionary<string, string> jwtImports = JsonConvert.DeserializeObject<Dictionary<string, string>>(configuration["jwt_cert"]);
                jwtCerts = new Dictionary<string, X509Certificate2>();
                jwtImports.ForEach(jwtCert => jwtCerts.Add(jwtCert.Key, new X509Certificate2(EIDResourceManager.ResolveResource(jwtCert.Value))));

                //Simple strings
                caCertificate = new X509Certificate2(EIDResourceManager.ResolveResource(configuration["ca_cert"]));
                clientCertificate = new X509Certificate2(EIDResourceManager.ResolveResource(configuration["client_cert"]), configuration["password"]);
                httpEndpoint = configuration["endpoint"];

                //Enums to make sure they are valid, stupidity?
                minimumLevel = (LOALevel)Enum.Parse(typeof(LOALevel), configuration["minimum_level"]);
                defaultCountry = (SSNCountry)Enum.Parse(typeof(SSNCountry), configuration["default_country"]);
                idType = (UserInfo)Enum.Parse(typeof(UserInfo), configuration["id_type"]);
                attributeList = (Attributes)Enum.Parse(typeof(Attributes), configuration["attribute_list"]);

            }
            catch
            {
                throw new ArgumentException("Configuration block was not valid");
            }
            
        }

        public override EIDResult CancelAuthRequest(string id)
        {
            JObject postData = new JObject();
            postData["authRef"] = id;
            string encodedData = "cancelAuthRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));
            return cancelRequest("authentication/1.0/cancel",encodedData);
        }

        public override EIDResult CancelSignRequest(string id)
        {
            JObject postData = new JObject();
            postData["signRef"] = id;
            string encodedData = "cancelSignRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));
            return cancelRequest("sign/1.0/cancel", encodedData);
        }

        public override EIDResult InitAuthRequest(string id)
        {
            JObject postData = new JObject();
            postData["minRegistrationLevel"] = minimumLevel.ToString();
            postData["userInfoType"] = idType.ToString();
            postData["signatureType"] = "SIMPLE";
            postData["dataToSignType"] = "SIMPLE_UTF8_TEXT";

            if (idType == UserInfo.SSN)
            {
                JObject userInfo = new JObject();
                userInfo["country"] = defaultCountry.ToString();
                userInfo["ssn"] = id;
                postData["userInfo"] = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(userInfo.ToString()));
            }
            else
                postData["userInfo"] = id;
            
            JArray attributeArray = new JArray();
            foreach (string attrib in attributeList.ToString().Replace(" ", "").Split(','))
            {
                JObject wrappingObject = new JObject();
                wrappingObject["attribute"] = attrib;
                attributeArray.Add(wrappingObject);
            }
            postData["attributesToReturn"] = attributeArray;

            string encodedData = "initAuthRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));

            return initRequest("authentication/1.0/initAuthentication", encodedData);
        }
        public override EIDResult InitSignRequest(string id, string text)
        {
            if (String.IsNullOrEmpty(text))
                return EIDResult.CreateErrorResult("request_text_invalid", "The supplied agreement text is not valid");

            JObject postData = new JObject();
            postData["minRegistrationLevel"] = minimumLevel.ToString();
            postData["userInfoType"] = idType.ToString();
            postData["signatureType"] = "SIMPLE";
            postData["dataToSignType"] = "SIMPLE_UTF8_TEXT";

            JObject dataToSign = new JObject();
            dataToSign["text"] = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(text));
            postData["dataToSign"] = dataToSign;

            if (idType == UserInfo.SSN)
            {
                JObject userInfo = new JObject();
                userInfo["country"] = defaultCountry.ToString();
                userInfo["ssn"] = id;
                postData["userInfo"] = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(userInfo.ToString()));
            }
            else
                postData["userInfo"] = id;

            JArray attributeArray = new JArray();
            foreach (string attrib in attributeList.ToString().Replace(" ", "").Split(','))
            {
                JObject wrappingObject = new JObject();
                wrappingObject["attribute"] = attrib;
                attributeArray.Add(wrappingObject);
            }
            postData["attributesToReturn"] = attributeArray;

            string encodedData = "initSignRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));

            return initRequest("sign/1.0/initSignature", encodedData);
        }

        public override EIDResult PollAuthRequest(string id)
        {
            JObject postData = new JObject();
            postData["authRef"] = id;
            string encodedData = "getOneAuthResultRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));
            return pollRequest("authentication/1.0/getOneResult", encodedData);
        }

        public override EIDResult PollSignRequest(string id)
        {
            JObject postData = new JObject();
            postData["signRef"] = id;
            string encodedData = "getOneSignResultRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));
            return pollRequest("sign/1.0/getOneResult", encodedData);
        }

        public bool CreateCustomIdentifier(string id, string customid)
        {
            JObject postData = new JObject();
            postData["userInfoType"] = idType.ToString();
            postData["customIdentifier"] = customid;

            if (idType == UserInfo.SSN)
            {
                JObject userInfo = new JObject();
                userInfo["country"] = defaultCountry.ToString();
                userInfo["ssn"] = id;
                postData["userInfo"] = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(userInfo.ToString()));
            }
            else
                postData["userInfo"] = id;

            string encodedData = "setCustomIdentifierRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));

            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/user/manage/1.0/setCustomIdentifier", encodedData).Result;

            if (httpResponse.HttpStatusCode == 204) return true;

            if (httpResponse.ContainsKey("message"))                
                throw new InvalidDataException(httpResponse["message"].ToString());

            throw new InvalidDataException(httpResponse.HttpStatusMessage);            
        }

        public bool DeleteCustomIdentifier(string customid)
        {
            JObject postData = new JObject();
            postData["customIdentifier"] = customid;

            string encodedData = "deleteCustomIdentifierRequest=" + System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(postData.ToString()));

            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/user/manage/1.0/deleteCustomIdentifier", encodedData).Result;

            if (httpResponse.HttpStatusCode == 204) return true;

            if (httpResponse.ContainsKey("message"))
                throw new InvalidDataException(httpResponse["message"].ToString());

            throw new InvalidDataException(httpResponse.HttpStatusMessage);
        }

        private EIDResult pollRequest(string endpoint, string postData)
        {
            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/" + endpoint, postData).Result;

            if (httpResponse.ContainsKey("status"))
            {
                switch (httpResponse["status"].ToString())
                {
                    case "EXPIRED":
                        return EIDResult.CreateErrorResult("expired_transaction", "The transaction was not completed in time");
                    case "DELIVERED_TO_MOBILE":
                        return EIDResult.CreatePendingResult("pending_user_in_app", "User have started the app");
                    case "STARTED":
                        return EIDResult.CreatePendingResult("pending_delivered", "Delivered to mobile phone");
                    case "CANCELED":
                    case "REJECTED":
                        return EIDResult.CreateErrorResult("cancelled_by_user", "The user declined transaction");
                    case "RP_CANCELED":
                        return EIDResult.CreateErrorResult("cancelled_by_idp", "The IdP have cancelled the request");
                    case "APPROVED":

                        JSonWebToken jsonWebToken = JSonWebToken.FromString(httpResponse["details"].ToString(), jwtCerts);

                        if (!jsonWebToken.IsValid)
                            return EIDResult.CreateErrorResult("api_error", "JWT Token validation failed"); ;

                        JObject requestedAttributes = (JObject)jsonWebToken.Payload["requestedAttributes"];

                        //Process name
                        string givenName = string.Empty;
                        string surName = string.Empty;
                        string fullName = string.Empty;
                        if (requestedAttributes.ContainsKey("basicUserInfo"))
                        {
                            givenName = requestedAttributes["basicUserInfo"]["name"].ToString();
                            surName = requestedAttributes["basicUserInfo"]["surname"].ToString();
                            fullName = givenName + " " + surName;
                        }

                        //Process identifier
                        string identifier = string.Empty;
                        if (jsonWebToken.Payload["userInfoType"].ToString() == "SSN")
                        {
                            JObject userInfo = JsonConvert.DeserializeObject<JObject>(jsonWebToken.Payload["userInfo"].ToString());
                            identifier = userInfo["ssn"].ToString();
                        }
                        else
                            identifier = jsonWebToken.Payload["userInfo"].ToString();

                        //Assemble basic response
                        JObject result = new JObject();
                        result["user"] = new JObject();
                        result["user"]["id"] = identifier;
                        result["user"]["firstname"] = givenName;
                        result["user"]["lastname"] = surName;
                        result["user"]["fullname"] = fullName;

                        result["extra"] = new JObject();
                        if (requestedAttributes.ContainsKey("dateOfBirth"))
                            result["extra"]["date_of_birth"] = requestedAttributes["dateOfBirth"].ToString();
                        if (requestedAttributes.ContainsKey("emailAddress"))
                            result["extra"]["email_address"] = requestedAttributes["emailAddress"].ToString();
                        if (requestedAttributes.ContainsKey("allEmailAddresses"))
                            result["extra"]["all_email_addresses"] = requestedAttributes["allEmailAddresses"].ToString();
                        if (requestedAttributes.ContainsKey("addresses"))
                            result["extra"]["addresses"] = requestedAttributes["addresses"].ToString();
                        if (requestedAttributes.ContainsKey("customIdentifier"))
                            result["extra"]["custom_identifier"] = requestedAttributes["customIdentifier"].ToString();
                        if (requestedAttributes.ContainsKey("ssn"))
                        {
                            result["extra"]["ssn_number"] = requestedAttributes["ssn"]["ssn"].ToString();
                            result["extra"]["ssn_country"] = requestedAttributes["ssn"]["country"].ToString();
                        }


                        return EIDResult.CreateCompletedResult(result);
                    default:
                        return EIDResult.CreateErrorResult("api_error", httpResponse["hintCode"].ToString());
                }
            }

            if (httpResponse.ContainsKey("code"))
            {
                switch (httpResponse["code"].ToString())
                {
                    case "1100":
                        return EIDResult.CreateErrorResult("request_id_invalid", "The supplied request cannot be found");
                    default:
                        return EIDResult.CreateErrorResult("api_error", httpResponse["message"].ToString());
                }

            }

            return EIDResult.CreateErrorResult("system_error", "A communications error occured", httpResponse.HttpStatusMessage);
        }

        private EIDResult cancelRequest(string endpoint, string postData)
        {
            HttpRequest httpRequest = new HttpRequest(caCertificate, clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/" + endpoint, postData).Result;
            return EIDResult.CreateCancelledResult();
        }

        private EIDResult initRequest(string endpoint, string postData)
        {
            // Make the request
            HttpRequest httpRequest = new HttpRequest(caCertificate,clientCertificate);
            HttpResponse httpResponse = httpRequest.Post(httpEndpoint + "/"+ endpoint, postData).Result;

            JObject result = new JObject();
            if (httpResponse.HttpStatusCode == 200)
            {

                if (httpResponse.ContainsKey("authRef")|| httpResponse.ContainsKey("signRef"))
                {
                    string refCode = httpResponse.ContainsKey("authRef") ? (string)httpResponse["authRef"] : (string)httpResponse["signRef"];
                    result["id"] = refCode;
                    result["extra"] = new JObject();
                    result["extra"]["autostart_token"] = refCode.ToString();
                    result["extra"]["autostart_url"] = "frejaeid://bindUserToTransaction?transactionReference=" + result["extra"]["autostart_token"];
                    return EIDResult.CreateInitializedResult(result);

                }

                return EIDResult.CreateErrorResult("api_error", "A communications error occured");

            }
            else {

                if (httpResponse.ContainsKey("code"))
                {
                    switch (httpResponse["code"].ToString())
                    {
                        case "2000":
                            return EIDResult.CreateErrorResult("already_in_progress", "A transaction was already pending for this SSN");
                        case "1001":
                        case "1002":
                            return EIDResult.CreateErrorResult("request_ssn_invalid", "The supplied SSN is not valid");
                        default:
                            return EIDResult.CreateErrorResult("api_error", "A communications error occured", httpResponse["message"].ToString());
                    }

                }

                return EIDResult.CreateErrorResult("system_error", "A communications error occured", httpResponse.HttpStatusMessage);

            }
        }
    }
}
