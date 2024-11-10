using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace UserAdminApp.Services
{
    public class DisableSSLValidationHandler : HttpClientHandler
    {
        public DisableSSLValidationHandler()
        {
            // Disable SSL certificate validation
            // Setting the ServerCertificateCustomValidationCallback is the correct approach
            this.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => true;
        }
    }

    public class DisableSSL
    {
        public static HttpClient GetHttpClient()
        {
            // Create a new HttpClient with the custom handler
            var handler = new DisableSSLValidationHandler();
            var client = new HttpClient(handler)
            {
                // Optionally, set the base address or other HttpClient configurations
                BaseAddress = new Uri("https://10.0.2.2:7148")
            };

            return client;
        }
    }
}
