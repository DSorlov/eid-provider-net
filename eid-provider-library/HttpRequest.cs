using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
   

    public class HttpRequest
    {
        private HttpClientHandler httpClientHandler;
        private HttpClient httpClient;

        private void InitializeCommon()
        {            
            httpClientHandler = new HttpClientHandler();
            httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void InitializeCustomRoots(X509Certificate2Collection rootCertificates)
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = CreateCustomRootValidator(rootCertificates);
        }

        private void InitializeClientCertAuthentication(X509Certificate2 clientCert)
        {
            httpClientHandler.ClientCertificates.Add(clientCert);
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        }

        private void InitializeClientBasicAuthentication(string username, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes(username+":"+password);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        internal HttpRequest(List<X509Certificate2> acceptedCACertificateList, X509Certificate2 clientCert)
        {
            InitializeCommon();
            InitializeCustomRoots(new X509Certificate2Collection(acceptedCACertificateList.ToArray()));
            InitializeClientCertAuthentication(clientCert);
        }

        internal HttpRequest(X509Certificate2 acceptedCACertificate, X509Certificate2 clientCert)
        {
            InitializeCommon();
            InitializeCustomRoots(new X509Certificate2Collection(acceptedCACertificate));
            InitializeClientCertAuthentication(clientCert);
        }

        internal HttpRequest(List<X509Certificate2> acceptedCACertificateList, string username, string password)
        {
            InitializeCommon();
            InitializeCustomRoots(new X509Certificate2Collection(acceptedCACertificateList.ToArray()));
            InitializeClientBasicAuthentication(username,password);
        }

        internal HttpRequest(string username, string password)
        {
            InitializeCommon();
            InitializeClientBasicAuthentication(username, password);
        }
        internal HttpRequest(X509Certificate2 clientCert)
        {
            InitializeCommon();
            InitializeClientCertAuthentication(clientCert);
        }

        internal async Task<HttpResponse> processResponse(HttpResponseMessage responseMessage)
        {

            string contentString = await responseMessage.Content.ReadAsStringAsync();
            JObject contentObject = null;

            try
            {
                contentObject = JObject.Parse(contentString);
            }
            catch
            {
                contentObject = new JObject();
            }


            return new HttpResponse(
                (int)responseMessage.StatusCode,
                responseMessage.ReasonPhrase,
                responseMessage.Headers.ToDictionary(l => l.Key, k => k.Value.ToString()),
                contentObject,
                contentString);
        }

        internal async Task<HttpResponse> Post(string uri, JObject data)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(data.ToString(), Encoding.UTF8, "application/json") ;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await httpClient.SendAsync(request) ;

            return await processResponse(response);
        }

        internal async Task<HttpResponse> Post(string uri, string data)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(data, Encoding.UTF8, "text/html");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            return await processResponse(response);
        }

        internal async Task<HttpResponse> Get(string uri)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri);

            return await processResponse(response);
        }

        internal static RemoteCertificateValidationCallback CreateCustomRootRemoteValidator(X509Certificate2Collection trustedRoots, X509Certificate2Collection intermediates = null)
        {
            if (trustedRoots == null)
                throw new ArgumentNullException(nameof(trustedRoots));
            if (trustedRoots.Count == 0)
                throw new ArgumentException("No trusted roots were provided", nameof(trustedRoots));

            // Let's avoid complex state and/or race conditions by making copies of these collections.
            // Then the delegates should be safe for parallel invocation (provided they are given distinct inputs, which they are).
            X509Certificate2Collection roots = new X509Certificate2Collection(trustedRoots);
            X509Certificate2Collection intermeds = null;

            if (intermediates != null)
            {
                intermeds = new X509Certificate2Collection(intermediates);
            }

            intermediates = null;
            trustedRoots = null;

            return (sender, serverCert, chain, errors) =>
            {
                // Missing cert or the destination hostname wasn't valid for the cert.
                if ((errors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                {
                    return false;
                }

                //dotNet 5.0 offers a nicer approach so if we are building modern then use that custom store otherwise just loop and hope the root is the first in the chain. =)
#if NET_50
                for (int i = 1; i < chain.ChainElements.Count; i++)
                {
                    chain.ChainPolicy.ExtraStore.Add(chain.ChainElements[i].Certificate);
                }

                if (intermeds != null)
                {
                    chain.ChainPolicy.ExtraStore.AddRange(intermeds);
                }

                chain.ChainPolicy.CustomTrustStore.Clear();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.AddRange(roots);
                return chain.Build((X509Certificate2)serverCert);
#else
                if (roots.Contains(chain.ChainElements[0].Certificate)) return true;

                foreach (X509Certificate2 cert in roots)
                    if (chain.ChainElements[0].Certificate.Issuer == cert.Subject) return true;

                return false;
# endif
           };
        }

        internal static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> CreateCustomRootValidator(X509Certificate2Collection trustedRoots, X509Certificate2Collection intermediates = null)
        {
            RemoteCertificateValidationCallback callback = CreateCustomRootRemoteValidator(trustedRoots, intermediates);
            return (message, serverCert, chain, errors) => callback(null, serverCert, chain, errors);
        }

    }
}
