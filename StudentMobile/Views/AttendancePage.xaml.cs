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
                // Notifications will be automatically loaded by LoadAttendanceAsync internally.
            }
        }
    }
}