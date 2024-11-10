using System;
using System.Net.Http;
using Xamarin.Forms;
using UserAdminApp.Services;
using UserAdminApp.Views;
using UserAdminApp.Droid;

namespace UserAdminApp
{
    public partial class App : Application
    {
        // Define HttpClient as a public property so it can be accessed globally
        public static HttpClient HttpClient { get; private set; }

        public App()
        {
            InitializeComponent();

            // Register the IHttpClientService with DependencyService for platform-specific HttpClient setup
            DependencyService.Register<IHttpClientService, HttpClientService>();

            // Get the HttpClient from the DependencyService
            HttpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();

            // Set base address based on platform (Android emulator uses 10.0.2.2 for localhost)
            HttpClient.BaseAddress = Device.RuntimePlatform == Device.Android
                                      ? new Uri("https://10.0.2.2:7148")
                                      : new Uri("https://localhost:7148");

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
