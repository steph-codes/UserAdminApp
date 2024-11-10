using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UserAdminApp.Services;
using Xamarin.Essentials;  // For Secure Storage
using Xamarin.Forms;

namespace UserAdminApp.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient; //= new HttpClient();

        public LoginPage()
        {
            InitializeComponent();
            // Use DependencyService to get platform-specific HttpClient (with SSL validation disabled for Android)
            _httpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();
            // Set base address for API requests
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148");  // Update this URL as needed
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            //string inputrole = RolePicker.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Please enter both username and password.", "OK");
                return;
            }

            var loginRequest = new { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/user/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string token = tokenResponse?.token;

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        await SecureStorage.SetAsync("auth_token", token);

                        var role = GetRoleFromToken(token);

                        if (role == "User")
                        {
                            await Navigation.PushAsync(new UserHomePage(username, token));
                        }
                        else if (role == "Admin")
                        {
                            await Navigation.PushAsync(new AdminHomePage(username, token));
                        }
                        else
                        {
                            await DisplayAlert("Error", "Role not recognized.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to retrieve token.", "OK");
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Login Failed", $"Error: {response.StatusCode} - {errorMessage}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }



        // Helper method to extract the role from the JWT token
        private string GetRoleFromToken(string token)
        {
            try
            {
                // Remove "Bearer " prefix if present
                if (token.StartsWith("Bearer "))
                {
                    token = token.Substring("Bearer ".Length).Trim();
                }

                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                // Read the JWT token
                var jsonToken = handler.ReadToken(token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                // Check if token is null or empty
                if (jsonToken == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid token.");
                    return null;
                }

                // Extract the role claim using the correct claim type
                var roleClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                // If found, return the role value; otherwise, return null
                return roleClaim?.Value;
            }
            catch (Exception ex)
            {
                // Log any errors during parsing
                System.Diagnostics.Debug.WriteLine($"Error parsing token: {ex.Message}");
                return null;
            }
        }


        // Event handler for navigating to Register Page
        private async void OnRegisterLabelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        // Event handler for navigating to Reset Password Page
        private async void OnResetPasswordLabelTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ResetPasswordPage());
        }
    }

}

