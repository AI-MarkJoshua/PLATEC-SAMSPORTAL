namespace AdminWebPage.Models
{
    public class AttendanceReportViewModel
    {
        public DateTime Date { get; set; }
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }

        public string ReportType { get; set; } // "Daily" or "Weekly"
    }
}
