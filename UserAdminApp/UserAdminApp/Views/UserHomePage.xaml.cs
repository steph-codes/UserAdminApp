using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials; // For SecureStorage
using static UserAdminApp.Views.UserProfilePage;
using UserAdminApp.Services; // Ensure to include the IHttpClientService namespace

namespace UserAdminApp.Views
{
    public partial class UserHomePage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly string _username;
        private readonly string _token;  // Store the JWT token if needed for authorization

        // Constructor now accepts IHttpClientService for proper HttpClient initialization
        public UserHomePage(string username, string token)
        {
            InitializeComponent();
            _username = username;
            _token = token;

            // Injecting the HttpClientService
            _httpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();

            // Set welcome message
            WelcomeLabel.Text = $"Welcome, {_username}!";

            // Set base address for API requests
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148");  // Use the correct address for your API
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token); // Authorization header with JWT token
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            

            // Optionally, show a confirmation message before logging out
            await DisplayAlert("Logged Out", "You have been logged out.", "OK");

            Logout();
        }

        // This will handle the back button for non-Android platforms
        protected override void OnDisappearing()
        {
            // Call Logout when navigating away from the page
            Logout();
            base.OnDisappearing();
        }

        // Logout Method
        private async void Logout()
        {
            // Clear the token from SecureStorage
            await SecureStorage.SetAsync("auth_token", string.Empty);

            // Navigate back to the login page (or you can push a login page depending on your flow)
            await Navigation.PushAsync(new LoginPage());
        }

        private async Task LogoutCall()
        {
            try
            {
                // Retrieve the token from SecureStorage
                var token = await SecureStorage.GetAsync("auth_token");

                if (!string.IsNullOrEmpty(token))
                {
                    // Set the token in the Authorization header
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Call the logout API endpoint
                    var response = await _httpClient.PostAsync("/api/auth/logout", null); // Adjust endpoint as needed

                    if (response.IsSuccessStatusCode)
                    {
                        // Successfully logged out, now remove the token from SecureStorage
                        await SecureStorage.SetAsync("auth_token", string.Empty);
                        await DisplayAlert("Success", "You have been logged out.", "OK");
                        await Navigation.PushAsync(new LoginPage());
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to log out. Please try again.", "OK");
                    }
                }
                else
                {
                    // If no token is found, proceed without calling the logout API
                    await DisplayAlert("Error", "No session found. You are not logged in.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            var userId = await GetUserIdFromUsername(_username);
            if (userId != null)
            {
                var user = await GetUserProfile(userId.Value);
                if (user != null)
                {
                    // Map User object to Userprofile object
                    var userProfile = new UserProfile
                    {
                        Username = user.Username,
                        Role = user.Role,
                        Userid = user.Userid
                    };
                    // Navigate to the Profile Page with the user profile details
                    await Navigation.PushAsync(new UserProfilePage(userProfile));
                }
                else
                {
                    await DisplayAlert("Error", "User profile not found.", "OK");
                }
            }
        }

        private async Task<int?> GetUserIdFromUsername(string username)
        {
            var response = await _httpClient.GetAsync($"/api/User/{username}");
            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);
                return user?.Userid;
            }
            return null;
        }

        private async Task<User> GetUserProfile(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/User/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userJson);
                return user;
            }
            return null;
        }
    }
}
