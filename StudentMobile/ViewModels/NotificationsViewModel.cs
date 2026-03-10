using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentMobile.Models;
using StudentMobile.Services;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudentMobile.ViewModels
{
    public partial class NotificationsViewModel : ObservableObject
    {
        private readonly NotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<NotificationRecord> notifications = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int unreadCount;

        [ObservableProperty]
        private bool hasNoNotifications;

        public NotificationsViewModel()
        {
            _notificationService = new NotificationService();
            LoadNotificationsAsync();
        }

        [RelayCommand]
        public async Task LoadNotificationsAsync()
        {
            IsLoading = true;

            try
            {
                if (Preferences.ContainsKey("StudentId"))
                {
                    int studentId = Preferences.Get("StudentId", 0);
                    
                    // Load from NotificationService
                    var serviceNotifications = await _notificationService.GetNotificationsAsync(studentId);
                    
                    Notifications.Clear();
                    foreach (var notification in serviceNotifications)
                    {
                        Notifications.Add(notification);
                    }
                    
                    UpdateUnreadCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex.Message}");
            }

            IsLoading = false;
        }

        [RelayCommand]
        public void MarkAsRead(NotificationRecord notification)
        {
            try
            {
                if (notification != null && _notificationService != null)
                {
                    notification.MarkAsRead();
                    UpdateUnreadCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        [RelayCommand]
        public void MarkAllAsRead()
        {
            try
            {
                foreach (var notification in Notifications.Where(n => !n.IsRead))
                {
                    notification.MarkAsRead();
                }
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking all as read: {ex.Message}");
            }
        }

        [RelayCommand]
        public void ClearAll()
        {
            try
            {
                Notifications.Clear();
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing notifications: {ex.Message}");
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = Notifications.Count(n => !n.IsRead);
            HasNoNotifications = !Notifications.Any();
        }
    }
}
