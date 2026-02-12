using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentMobile.Models;
using StudentMobile.Services;
using Microsoft.Maui.Storage;


namespace StudentMobile.ViewModels
{
    public partial class AttendanceViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private List<AttendanceRecord> attendanceRecords = new();

        [ObservableProperty]
        private bool isLoading;

        public AttendanceViewModel()
        {
            _apiService = new ApiService();
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
                    
                    System.Diagnostics.Debug.WriteLine($"Loaded {AttendanceRecords.Count} attendance records");
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

            IsLoading = false;
        }
    }
}
