using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UserAdminApp.Services;
using Xamarin.Forms;

namespace UserAdminApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public RegisterPage()
        {
            InitializeComponent();
            // Use the same HttpClient instance to bypass SSL validation
            _httpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148");  // Update this URL as needed
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            // Capture user input
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string role = RolePicker.SelectedItem?.ToString();

            // Validate input fields
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            // Create the registration request object
            var registerRequest = new
            {
                Username = username,
                Password = password,
                Role = role
            };

            // Serialize the object to JSON
            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request to the register endpoint
                var response = await _httpClient.PostAsync("/api/user/register", content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Registration successful! Please log in.", "OK");
                    // Navigate back to LoginPage
                    await Navigation.PushAsync(new LoginPage());
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Registration Failed", $"Error: {response.StatusCode} - {errorMessage}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
