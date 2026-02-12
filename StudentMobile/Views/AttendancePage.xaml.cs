using StudentMobile.ViewModels;

namespace StudentMobile.Views;

public partial class AttendancePage : ContentPage
{
	public AttendancePage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        var viewModel = BindingContext as AttendanceViewModel;
        if (viewModel != null)
        {
            await viewModel.LoadAttendanceAsync();
        }
    }
}