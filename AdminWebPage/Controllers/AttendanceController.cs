using AdminWebPage.Data;
using AdminWebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace AdminWebPage.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AdminWebPageContext _context;

        public AttendanceController(AdminWebPageContext context)
        {
            _context = context;
        }
        // GET: Attendance
        public async Task<IActionResult> Index()
        {
            var students = await _context.Account
                .Where(a => a.Role == "Student")
                .ToListAsync();
            return View(students);
        }

        // POST: Attendance/Mark
        [HttpPost]
        public async Task<IActionResult> Mark(int studentId, string status)
        {
            var today = DateTime.Now.Date;

            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date == today);

            if (existing != null)
            {
                existing.Status = status;
                _context.Update(existing);
            }
            else
            {
                var attendance = new Attendance
                {
                    StudentId = studentId,
                    Date = today,
                    Status = status
                };
                _context.Add(attendance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reports(string type = "Daily")
        {
            var today = DateTime.Now.Date;
            List<AttendanceReportViewModel> reportData = new List<AttendanceReportViewModel>();

            if (type == "Daily")
            {
                var dailyAttendance = await _context.Attendances
                    .Where(a => a.Date == today)
                    .ToListAsync();

                reportData.Add(new AttendanceReportViewModel
                {
                    Date = today,
                    TotalStudents = await _context.Account.CountAsync(a => a.Role == "Student"),
                    PresentCount = dailyAttendance.Count(a => a.Status == "Present"),
                    AbsentCount = dailyAttendance.Count(a => a.Status == "Absent"),
                    LateCount = dailyAttendance.Count(a => a.Status == "Late"),
                    ReportType = "Daily"
                });
            }
            else if (type == "Weekly")
            {
                // Assuming week starts on Monday
                var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var weekStart = today.AddDays(-1 * diff);
                var weekEnd = weekStart.AddDays(6);

                for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
                {
                    var dailyAttendance = await _context.Attendances
                        .Where(a => a.Date == date)
                        .ToListAsync();

                    reportData.Add(new AttendanceReportViewModel
                    {
                        Date = date,
                        TotalStudents = await _context.Account.CountAsync(a => a.Role == "Student"),
                        PresentCount = dailyAttendance.Count(a => a.Status == "Present"),
                        AbsentCount = dailyAttendance.Count(a => a.Status == "Absent"),
                        LateCount = dailyAttendance.Count(a => a.Status == "Late"),
                        ReportType = "Weekly"
                    });
                }
            }

            ViewBag.ReportType = type;
            return View(reportData);
        }
    }
}
