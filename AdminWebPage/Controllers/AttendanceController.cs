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
        public async Task<IActionResult> Index(DateTime? selectedDate)
        {
            ViewBag.SelectedDate = selectedDate;

            if (selectedDate == null)
            {
                return View(new List<Account>());
            }

            var students = await _context.Account
                .Where(a => a.Role == "Student")
                .ToListAsync();

            return View(students);
        }


        // POST: Attendance/Mark
        [HttpPost]
        public async Task<IActionResult> Mark(int studentId, string status, DateTime date)
        {
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date == date);

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
                    Date = date,
                    Status = status
                };
                _context.Add(attendance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { selectedDate = date });
        }


        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
        {
            var reportData = new List<AttendanceReportViewModel>();

            if (startDate == null || endDate == null)
            {
                return View(reportData);
            }

            for (var date = startDate.Value; date <= endDate.Value; date = date.AddDays(1))
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
                });
            }

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(reportData);
        }

    }
}
