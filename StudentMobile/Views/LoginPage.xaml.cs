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
                await ShowMessageOverlay("Please enter both username and password", "message_icon.svg", 2000);
                return;
            }

            // Show loading overlay
            await ShowLoadingOverlay("Logging in...");

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
                    // Save student information using Preferences
                    Preferences.Set("StudentId", result.AccountID);
                    
                    // Use full name from login response (no need for separate API call)
                    string fullName = $"{result.FName} {result.LName}";
                    Preferences.Set("StudentName", fullName);
                    
                    // Show success message
                    await ShowMessageOverlay("Login successful!", "success_icon.svg", 3000);

                    // Navigate to AttendancePage
                    var attendancePage = new Views.AttendancePage();
                    Application.Current.MainPage = attendancePage;
                }
                else
                {
                    // Show access denied message
                    await ShowMessageOverlay("Access denied. Only students can login.", "message_icon.svg", 3000);
                }
            }
            else
            {
                // Show login failed message
                await ShowMessageOverlay("Invalid username or password.", "message_icon.svg", 3000);
            }
        }
        catch (Exception ex)
        {
            await ShowMessageOverlay($"Login error: {ex.Message}", "message_icon.svg", 3000);
        }
    }

    private async Task ShowLoadingOverlay(string message)
    {
        LoadingMessage.Text = message;
        LoadingIcon.Source = "loading_icon.svg";
        LoadingFrame.IsVisible = true;
        MessageFrame.IsVisible = false;
        LoadingOverlay.IsVisible = true;
        
        await Task.Delay(2000);
        
        LoadingOverlay.IsVisible = false;
    }

    private async Task ShowMessageOverlay(string message, string iconName, int delayMs)
    {
        MessageText.Text = message;
        MessageIcon.Source = iconName;
        LoadingFrame.IsVisible = false;
        MessageFrame.IsVisible = true;
        LoadingOverlay.IsVisible = true;
        
        await Task.Delay(delayMs);
        
        LoadingOverlay.IsVisible = false;
    }
}