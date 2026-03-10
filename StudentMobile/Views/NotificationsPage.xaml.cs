using StudentMobile.ViewModels;

namespace StudentMobile.Views
{
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            var viewModel = BindingContext as NotificationsViewModel;
            if (viewModel != null)
            {
                await viewModel.LoadNotificationsAsync();
            }
        }
    }
}
