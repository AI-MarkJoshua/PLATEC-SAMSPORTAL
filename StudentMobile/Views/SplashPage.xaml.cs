using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace StudentMobile.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Give the full-screen image a couple of seconds to display
        await Task.Delay(2000);

        // Navigate to the main application flow (assuming LoginPage is the entry)
        Application.Current.MainPage = new LoginPage();
    }
}
