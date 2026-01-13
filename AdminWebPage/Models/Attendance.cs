namespace AdminWebPage.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // Present, Absent, Late

        public Account Student { get; set; }
    }
}
