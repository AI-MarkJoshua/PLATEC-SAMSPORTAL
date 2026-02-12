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

            if (Preferences.ContainsKey("StudentId"))
            {
                int studentId = Preferences.Get("StudentId", 0);
                AttendanceRecords = await _apiService.GetMyAttendanceAsync(studentId);
            }


            IsLoading = false;
        }
    }
}
