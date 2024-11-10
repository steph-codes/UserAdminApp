using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Xamarin.Forms;
using Xamarin.Essentials;
using static UserAdminApp.Views.UserProfilePage;

namespace UserAdminApp.Views
{
    public partial class UserProfilePage : ContentPage
    {
        private readonly HttpClient _httpClient = new HttpClient();

        // Add a constructor that accepts username
        public UserProfilePage(UserProfile UserProfile)
        {
            InitializeComponent();

            // Display the username on the page if needed
            //WelcomeLabel.Text = $"Welcome {UserProfile.Username}!";
            RoleLabel.Text = $"Role: {UserProfile.Role}";

            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148");  // Adjust your API URL accordingly
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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

        // This is called when the "Fetch Profile" button is clicked
        private async void OnFetchProfileClicked(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the token from SecureStorage
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "You are not authenticated. Please log in again.", "OK");
                    return;
                }

                // Set the token in the Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Make the API call to get the user profile
                var response = await _httpClient.GetAsync("/api/user/profile");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userProfile = JsonConvert.DeserializeObject<UserProfile>(responseContent);

                    // Update the UI with the user profile data
                    UsernameLabel.Text = userProfile.Username;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to fetch user profile.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Define a class to deserialize the profile data
        public class UserProfile
        {
            public string Username { get; set; }
            public string Role { get; set; }
            public int Userid { get; set; }
        }
    }
}
