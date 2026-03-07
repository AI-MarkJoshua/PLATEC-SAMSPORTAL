using StudentMobile.Services;
using StudentMobile.Models;
using Microsoft.Maui.Storage;

namespace StudentMobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Forgot Password", "Password reset functionality will be implemented soon.", "OK");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var username = UsernameEntry?.Text?.Trim();
            var password = PasswordEntry?.Text?.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Validation Error", "Please enter both username and password.", "OK");
                return;
            }

            // Show loading alert
            await DisplayAlert("Login", "Logging in...", "OK");

            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var result = await _apiService.LoginAsync(loginRequest);

            if (result != null)
            {
                if (result.Role == "Student")
                {
                    // Save student ID using Preferences
                    Preferences.Set("StudentId", result.AccountID);

                    await DisplayAlert("Success", "Login successful!", "OK");

                    // Navigate to AttendancePage
                    var attendancePage = new Views.AttendancePage();
                    Application.Current.MainPage = attendancePage;
                }
                else
                {
                    await DisplayAlert("Access Denied", "Access denied. Only students can login.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Login Failed", "Invalid username or password.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Login Error", $"Login error: {ex.Message}", "OK");
        }
    }
}