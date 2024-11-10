using System.Net.Http;
using Xamarin.Forms;
using UserAdminApp.Droid; // Reference to your Android namespace
using UserAdminApp.Services; // Reference to your shared project

[assembly: Dependency(typeof(HttpClientService))]
namespace UserAdminApp.Droid
{
    public class HttpClientService : IHttpClientService
    {
        public HttpClient GetHttpClient()
        {
            // Create HttpClient using the custom handler that disables SSL validation
            var handler = new DisableSSLValidationHandler();
            var client = new HttpClient(handler);
            return client;
        }
    }
}
