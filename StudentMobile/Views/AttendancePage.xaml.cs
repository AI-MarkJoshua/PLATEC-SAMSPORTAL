using StudentMobile.ViewModels;
using StudentMobile.Services;

namespace StudentMobile.Views
{
    public partial class AttendancePage : ContentPage
    {
        private readonly NotificationService _notificationService;

        public AttendancePage()
        {
            InitializeComponent();
            _notificationService = new NotificationService();
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

        private async void OnNotificationsClicked(object sender, EventArgs e)
        {
            try
            {
                var notificationsPage = new Views.NotificationsPage();
                await Navigation.PushAsync(notificationsPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to notifications: {ex.Message}");
                await DisplayAlert("Error", "Unable to open notifications", "OK");
            }
        }
    }
}