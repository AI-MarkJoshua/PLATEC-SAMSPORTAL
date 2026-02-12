using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentMobile.Models;
using StudentMobile.Services;
using Microsoft.Maui.Storage;

namespace StudentMobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string loginMessage;

        public LoginViewModel()
        {
            _apiService = new ApiService();
        }

        [RelayCommand]
        public async Task TestConnectionAsync()
        {
            try
            {
                LoginMessage = "Testing connection...";
                
                // Test 1: Using our ApiService
                var response1 = await _apiService.TestConnectionAsync();
                
                // Test 2: Using simple test
                var response2 = await SimpleApiTest.TestApiCall();
                
                LoginMessage = $"ApiService: {(response1 ? "✅" : "❌")} | SimpleTest: {response2}";
            }
            catch (Exception ex)
            {
                LoginMessage = $"❌ Connection error: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            try
            {
                LoginMessage = "Logging in...";
                
                var loginRequest = new LoginRequest
                {
                    Username = Username,
                    Password = Password
                };

                var result = await _apiService.LoginAsync(loginRequest);

                if (result != null)
                {
                    LoginMessage = $"Login successful! Role: {result.Role}";
                    
                    if (result.Role == "Student")
                    {
                        // Save student ID using Preferences instead of App.Current.Properties
                        Preferences.Set("StudentId", result.AccountID);

                        // Navigate to AttendancePage using Application.Current
                        var attendancePage = new Views.AttendancePage();
                        Application.Current.MainPage = attendancePage;
                    }
                    else
                    {
                        LoginMessage = "Access denied. Only students can login.";
                    }
                }
                else
                {
                    LoginMessage = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Login error: {ex.Message}";
            }
        }
    }
}
