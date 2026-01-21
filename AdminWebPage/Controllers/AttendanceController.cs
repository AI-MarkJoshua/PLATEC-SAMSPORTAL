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

            var students = await _context.Account
                .Where(a => a.Role == "Student")
                .ToListAsync();

            // Load attendance for selected date
            var attendanceDict = new Dictionary<int, string>();
            if (selectedDate != null)
            {
                attendanceDict = await _context.Attendances
                    .Where(a => a.Date >= selectedDate.Value.Date
                             && a.Date < selectedDate.Value.Date.AddDays(1))
                    .ToDictionaryAsync(a => a.StudentId, a => a.Status);
            }
            ViewBag.AttendanceMap = attendanceDict;

            // Get all attendance dates
            var attendanceDates = await _context.Attendances
                .Select(a => a.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            ViewBag.AttendanceDates = attendanceDates;

            // Load all attendances grouped by date for modal display
            var allAttendance = await _context.Attendances
                .Include(a => a.Student)
                .ToListAsync();

            // Group by date
            var attendanceByDate = allAttendance
                .GroupBy(a => a.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.StudentId, a => a.Status));

            ViewBag.AttendanceByDate = attendanceByDate;

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

        // POST: Attendance/MarkAll
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> MarkAll([FromBody] List<AttendanceDto> attendances)
        {
            foreach (var item in attendances)
            {
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == item.StudentId
                                           && a.Date >= item.Date.Date
                                           && a.Date < item.Date.Date.AddDays(1));

                if (existing != null)
                {
                    existing.Status = item.Status;
                    _context.Update(existing);
                }
                else
                {
                    _context.Add(new Attendance
                    {
                        StudentId = item.StudentId,
                        Date = item.Date.Date, // save only date part
                        Status = item.Status
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
        {
            var reportData = new List<AttendanceReportViewModel>();
            var detailedList = new List<dynamic>(); // list of students with date and status

            if (startDate == null || endDate == null)
                return View(reportData);

            var students = await _context.Account
                .Where(a => a.Role == "Student")
                .ToListAsync();

            for (var date = startDate.Value.Date; date <= endDate.Value.Date; date = date.AddDays(1))
            {
                var dailyAttendance = await _context.Attendances
                    .Where(a => a.Date >= date && a.Date < date.AddDays(1))
                    .Include(a => a.Student)
                    .ToListAsync();

                reportData.Add(new AttendanceReportViewModel
                {
                    Date = date,
                    TotalStudents = students.Count,
                    PresentCount = dailyAttendance.Count(a => a.Status == "Present"),
                    AbsentCount = dailyAttendance.Count(a => a.Status == "Absent"),
                    LateCount = dailyAttendance.Count(a => a.Status == "Late"),
                });

                // Fill detailed list
                foreach (var student in students)
                {
                    var status = dailyAttendance.FirstOrDefault(a => a.StudentId == student.AccountID)?.Status ?? "N/A";

                    detailedList.Add(new
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        student.FName,
                        student.MName,
                        student.LName,
                        Remarks = status
                    });
                }
            }

            ViewBag.DetailedList = detailedList;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(reportData);
        }



    }
}
