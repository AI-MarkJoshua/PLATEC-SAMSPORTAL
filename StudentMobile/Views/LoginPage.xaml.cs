using StudentMobile.Services;

namespace StudentMobile.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await SimpleApiTest.TestApiCall();
            await DisplayAlert("Connection Test", result, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Debug", "Login button clicked!", "OK");
            
            var viewModel = BindingContext as ViewModels.LoginViewModel;
            if (viewModel != null)
            {
                await viewModel.LoginAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Login Error", ex.Message, "OK");
        }
    }
}