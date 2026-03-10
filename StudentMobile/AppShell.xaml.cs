using StudentMobile.Views;

namespace StudentMobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register pages
            Routing.RegisterRoute("Notifications", typeof(NotificationsPage));
        }
    }
}
