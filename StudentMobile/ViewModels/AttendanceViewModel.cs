using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentMobile.Models;
using StudentMobile.Services;
using Microsoft.Maui.Storage;
using System.Linq;
using System.Collections.ObjectModel;

namespace StudentMobile.ViewModels
{
    public partial class AttendanceViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private List<AttendanceRecord> attendanceRecords = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int presentCount;

        [ObservableProperty]
        private int absentCount;

        [ObservableProperty]
        private int lateCount;

        [ObservableProperty]
        private bool isNotificationOverlayVisible;

        [ObservableProperty]
        private int unreadNotificationCount;

        [ObservableProperty]
        private bool hasUnreadNotifications;

        [ObservableProperty]
        private ObservableCollection<NotificationRecord> notifications = new();

        private readonly NotificationService _notificationService;

        public AttendanceViewModel()
        {
            _apiService = new ApiService();
            _notificationService = new NotificationService();
        }

        [RelayCommand]
        public async Task LoadAttendanceAsync()
        {
            IsLoading = true;

            try
            {
                if (Preferences.ContainsKey("StudentId"))
                {
                    int studentId = Preferences.Get("StudentId", 0);
                    System.Diagnostics.Debug.WriteLine($"Loading attendance for StudentId: {studentId}");
                    
                    AttendanceRecords = await _apiService.GetMyAttendanceAsync(studentId);
                    
                    // Calculate statistics
                    PresentCount = AttendanceRecords.Count(a => a.Status == "Present");
                    AbsentCount = AttendanceRecords.Count(a => a.Status == "Absent");
                    LateCount = AttendanceRecords.Count(a => a.Status == "Late");
                    
                    System.Diagnostics.Debug.WriteLine($"Loaded {AttendanceRecords.Count} attendance records");
                    System.Diagnostics.Debug.WriteLine($"Present: {PresentCount}, Absent: {AbsentCount}, Late: {LateCount}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No StudentId found in preferences");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading attendance: {ex.Message}");
            }

            await LoadNotificationsAsync();

            IsLoading = false;
        }

        public async Task LoadNotificationsAsync()
        {
            try
            {
                if (Preferences.ContainsKey("StudentId"))
                {
                    int studentId = Preferences.Get("StudentId", 0);
                    var notifs = await _notificationService.GetNotificationsAsync(studentId);
                    
                    Notifications.Clear();
                    UnreadNotificationCount = 0;

                    if (notifs != null)
                    {
                        foreach (var notification in notifs)
                        {
                            Notifications.Add(notification);
                            if (!notification.IsRead)
                            {
                                UnreadNotificationCount++;
                            }
                        }
                    }
                    
                    HasUnreadNotifications = UnreadNotificationCount > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading notifications: {ex.Message}");
            }
        }

        [RelayCommand]
        public void ToggleNotificationOverlay()
        {
            IsNotificationOverlayVisible = !IsNotificationOverlayVisible;

            if (IsNotificationOverlayVisible && HasUnreadNotifications)
            {
                // Mark all as read when opening
                foreach (var notification in Notifications)
                {
                    if (!notification.IsRead)
                    {
                        notification.MarkAsRead();
                    }
                }
                UnreadNotificationCount = 0;
                HasUnreadNotifications = false;
            }
        }

        [RelayCommand]
        public void CloseNotificationOverlay()
        {
            IsNotificationOverlayVisible = false;
        }
    }
}
