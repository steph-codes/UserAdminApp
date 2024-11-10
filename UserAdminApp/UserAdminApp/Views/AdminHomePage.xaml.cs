using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using UserAdminApp.Services;
using Xamarin.Essentials;

namespace UserAdminApp.Views
{
    public partial class AdminHomePage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public AdminHomePage(string username, string token)
        {
            InitializeComponent();

            _httpClient = DependencyService.Get<IHttpClientService>().GetHttpClient();
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7148");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _token = token;

            WelcomeLabel.Text = $"Welcome Admin, {username}!";
        }


        // Override OnBackPressed method for Android
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Call the logout endpoint when the back button is pressed
            await Logout();
            await Navigation.PushAsync(new LoginPage());
        }

        // Method to logout, calling the logout API and removing the token from SecureStorage
        private async Task Logout()
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

        // Load and display users when "View Users" is clicked
        private async void OnViewUsersClicked(object sender, EventArgs e)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = await _httpClient.GetAsync("api/user/all");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<User>>(responseContent);
                    UsersListView.ItemsSource = users;
                    UsersListView.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to load users.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Update user when "Edit User" is clicked
        private async void OnEditUserClicked(object sender, EventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as User;
            if (selectedUser == null)
            {
                await DisplayAlert("Error", "Please select a user to edit.", "OK");
                return;
            }

            // Create a prompt for updating user details
            string newRole = await DisplayPromptAsync("Edit Role", "Enter new role for user:");

            if (!string.IsNullOrWhiteSpace(newRole))
            {
                selectedUser.Role = newRole; // Update the role property

                try
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    // Convert updated user data to JSON
                    var userJson = JsonConvert.SerializeObject(selectedUser);
                    var content = new StringContent(userJson, Encoding.UTF8, "application/json");

                    // Send PUT request to update the user
                    var response = await _httpClient.PutAsync($"api/user/update/{selectedUser.Userid}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "User updated successfully.", "OK");
                        OnViewUsersClicked(sender, e); // Refresh user list
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to update user.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }

        // Delete selected user when "Delete User" is clicked
        private async void OnDeleteUserClicked(object sender, EventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as User;
            if (selectedUser == null)
            {
                await DisplayAlert("Error", "Please select a user to delete.", "OK");
                return;
            }

            bool confirmDelete = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {selectedUser.Username}?", "Yes", "No");
            if (confirmDelete)
            {
                try
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    var response = await _httpClient.DeleteAsync($"api/user/delete/{selectedUser.Userid}");

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "User deleted successfully.", "OK");
                        OnViewUsersClicked(sender, e); // Refresh user list
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to delete user.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
            }
        }

        // Fetch selected user details when "Select User" is clicked
        private async void OnSelectUserClicked(object sender, EventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as User;
            if (selectedUser == null)
            {
                await DisplayAlert("Error", "Please select a user from the list.", "OK");
                return;
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = await _httpClient.GetAsync($"api/user/{selectedUser.Userid}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var userDetails = JsonConvert.DeserializeObject<User>(responseContent);

                    await DisplayAlert("User Details",
                        $"Username: {userDetails.Username}\nRole: {userDetails.Role}",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to fetch user details.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnUserSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedUser = e.SelectedItem as User;
            if (selectedUser != null)
            {
                await DisplayAlert("User Selected", $"Selected User: {selectedUser.Username}", "OK");
                ((ListView)sender).SelectedItem = null;
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public int Userid { get; set; }
    }
}
