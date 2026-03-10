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
                
                // Show notifications as popups
                await ShowAttendanceNotificationsAsync();
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

        private async Task ShowAttendanceNotificationsAsync()
        {
            try
            {
                if (Preferences.ContainsKey("StudentId"))
                {
                    int studentId = Preferences.Get("StudentId", 0);
                    var notifications = await _notificationService.GetNotificationsAsync(studentId);
                    
                    // Show each notification as a popup
                    foreach (var notification in notifications.Where(n => !n.IsRead))
                    {
                        await DisplayAlert(
                            notification.Title,
                            notification.Message,
                            "OK");
                        
                        // Mark as read so it doesn't show again
                        notification.MarkAsRead();
                        
                        // Small delay between notifications
                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notifications: {ex.Message}");
            }
        }
    }
}