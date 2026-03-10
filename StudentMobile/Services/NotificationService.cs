using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentMobile.Models;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

namespace StudentMobile.Services
{
    public partial class NotificationService : ObservableObject
    {
        private readonly ApiService _apiService;
        private Timer _notificationTimer;
        
        [ObservableProperty]
        private ObservableCollection<NotificationRecord> notifications = new();
        
        [ObservableProperty]
        private int unreadCount;

        [ObservableProperty]
        private bool hasNewNotifications;

        public NotificationService()
        {
            _apiService = new ApiService();
            InitializeNotifications();
        }

        private void InitializeNotifications()
        {
            // Load existing notifications
            LoadStoredNotifications();
            
            // Start periodic check for new notifications
            _notificationTimer = new Timer(CheckForNewNotifications, null, 
                TimeSpan.Zero, TimeSpan.FromMinutes(5)); // Check every 5 minutes
        }

        private async void CheckForNewNotifications(object state)
        {
            try
            {
                if (Preferences.ContainsKey("StudentId"))
                {
                    int studentId = Preferences.Get("StudentId", 0);
                    var newNotifications = await _apiService.GetNotificationsAsync(studentId);
                    
                    if (newNotifications?.Any() == true)
                    {
                        foreach (var notification in newNotifications)
                        {
                            if (!notifications.Any(n => n.Id == notification.Id))
                            {
                                notifications.Insert(0, notification);
                                await TriggerNotificationAlert(notification);
                            }
                        }
                        
                        UpdateUnreadCount();
                        SaveNotifications();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking notifications: {ex.Message}");
            }
        }

        public async Task<List<NotificationRecord>> GetNotificationsAsync(int studentId)
        {
            try
            {
                var apiService = new ApiService();
                return await apiService.GetNotificationsAsync(studentId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting notifications: {ex.Message}");
                return new List<NotificationRecord>();
            }
        }

        private async Task TriggerNotificationAlert(NotificationRecord notification)
        {
            // Show local notification
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(
                    notification.Title,
                    notification.Message,
                    "OK"
                );
            });
        }

        private void LoadStoredNotifications()
        {
            try
            {
                var storedNotifications = Preferences.Get("StoredNotifications", "");
                if (!string.IsNullOrEmpty(storedNotifications))
                {
                    var notificationList = System.Text.Json.JsonSerializer.Deserialize<List<NotificationRecord>>(storedNotifications);
                    if (notificationList != null)
                    {
                        foreach (var notification in notificationList)
                        {
                            notifications.Add(notification);
                        }
                    }
                }
                UpdateUnreadCount();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading stored notifications: {ex.Message}");
            }
        }

        private void SaveNotifications()
        {
            try
            {
                var notificationsJson = System.Text.Json.JsonSerializer.Serialize(notifications.ToList());
                Preferences.Set("StoredNotifications", notificationsJson);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving notifications: {ex.Message}");
            }
        }

        private void UpdateUnreadCount()
        {
            UnreadCount = notifications.Count(n => !n.IsRead);
            HasNewNotifications = UnreadCount > 0;
        }

        [RelayCommand]
        public void MarkAsRead(NotificationRecord notification)
        {
            notification.IsRead = true;
            UpdateUnreadCount();
            SaveNotifications();
        }

        [RelayCommand]
        public void MarkAllAsRead()
        {
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            UpdateUnreadCount();
            SaveNotifications();
        }

        [RelayCommand]
        public void ClearAllNotifications()
        {
            notifications.Clear();
            UpdateUnreadCount();
            SaveNotifications();
        }

        public void Dispose()
        {
            _notificationTimer?.Dispose();
        }
    }
}
