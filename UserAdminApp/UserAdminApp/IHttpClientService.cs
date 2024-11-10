using System.Net.Http;

namespace UserAdminApp.Services
{
    public interface IHttpClientService
    {
        HttpClient GetHttpClient();
    }
}
