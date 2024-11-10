using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UserAdminApp.Services;
using Xamarin.Forms;

namespace UserAdminApp.Views
{
    public partial class ResetPasswordPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public ResetPasswordPage()
        {
            InitializeComponent();

            // Instantiate HttpClient with SSL bypass as in the LoginPage
            _httpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148"); // Base URL
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async void OnResetPasswordButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string newPassword = NewPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
            {
                await DisplayAlert("Error", "Please enter both username and new password.", "OK");
                return;
            }

            var resetPasswordRequest = new { Username = username, NewPassword = newPassword };
            var json = JsonConvert.SerializeObject(resetPasswordRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/user/reset-password", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Password reset successful! Please login with your new password.", "OK");
                    await Navigation.PushAsync(new LoginPage()); // Navigate back to the login page
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Reset Password Failed", $"Error: {response.StatusCode} - {errorMessage}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
