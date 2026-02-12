using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using StudentMobile.Models;
using StudentMobile.Services;

namespace StudentMobile.ViewModels
{
    public class AttendanceViewModel : BindableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<string> StatusOptions { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }

        public AttendanceViewModel()
        {
            _apiService = new ApiService();

            Students = new ObservableCollection<Student>();
            StatusOptions = new ObservableCollection<string>
            {
                "Present",
                "Absent"
            };

            LoadCommand = new Command(async () => await LoadStudents());
            SaveCommand = new Command(async () => await SaveAttendance());
        }

        private async Task LoadStudents()
        {
            var students = await _apiService.GetStudentsAsync();
            Students.Clear();

            foreach (var student in students)
            {
                Students.Add(student);
            }
        }

        private async Task SaveAttendance()
        {
            var attendanceList = Students.Select(s => new AttendanceDto
            {
                StudentId = s.AccountID,
                Status = "Present", // default for now
                Date = DateTime.Today
            }).ToList();

            await _apiService.MarkAllAttendanceAsync(attendanceList);
        }
    }
}

