using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AdminWebPageContext _context;
        private readonly HttpClient _httpClient;

        public DashboardController(AdminWebPageContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        private string BaseUrl => "https://api.example.com"; // replace with your base URL

        public async Task<IActionResult> Index(string view = "daily")
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check if user is logged in
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }

            // 🔢 COUNTS - Different based on role
            if (userRole == "Admin")
            {
                ViewBag.TotalStudents = await _context.Accounts
                    .CountAsync(a => a.Role == "Student");

                ViewBag.TotalTeachers = await _context.Accounts
                    .CountAsync(a => a.Role == "Teacher");

                ViewBag.TotalAdmins = await _context.Accounts
                    .CountAsync(a => a.Role == "Admin");

                // New metrics for admin
                ViewBag.TotalSubjects = await _context.Sections.CountAsync();
                ViewBag.TotalAttendanceCreated = await _context.Attendances.CountAsync();
            }
            else if (userRole == "Teacher")
            {
                ViewBag.TotalStudents = await _context.Accounts
                    .CountAsync(a => a.Role == "Student" && a.TeacherID == accountId);
                    
                ViewBag.TotalTeachers = 1; // Only themselves
                ViewBag.TotalAdmins = await _context.Accounts
                    .CountAsync(a => a.Role == "Admin");
            }
            else // Student
            {
                ViewBag.TotalStudents = await _context.Accounts
                    .CountAsync(a => a.Role == "Student");

                ViewBag.TotalTeachers = await _context.Accounts
                    .CountAsync(a => a.Role == "Teacher");

                ViewBag.TotalAdmins = await _context.Accounts
                    .CountAsync(a => a.Role == "Admin");
            }

            // ✅ NEW: TODAY PRESENT & ABSENT
            var today = DateTime.Today;

            ViewBag.PresentToday = await _context.Attendances
                .CountAsync(a =>
                    a.Date >= today &&
                    a.Date < today.AddDays(1) &&
                    a.Status == "Present");

            ViewBag.AbsentToday = await _context.Attendances
                .CountAsync(a =>
                    a.Date >= today &&
                    a.Date < today.AddDays(1) &&
                    a.Status == "Absent");

            ViewBag.LateToday = await _context.Attendances
                .CountAsync(a =>
                    a.Date >= today &&
                    a.Date < today.AddDays(1) &&
                    a.Status == "Late");

            // 📅 DATE RANGE
            DateTime startDate;
            DateTime endDate = DateTime.Today;

            if (view == "weekly")
                startDate = DateTime.Today.AddDays(-6);
            else
                startDate = DateTime.Today;

            // ATTENDANCE CHART DATA
            if (userRole == "Teacher")
            {
                // Teacher sees their students with individual attendance status
                var teacherStudents = await _context.Accounts
                    .Where(a => a.Role == "Student" && a.TeacherID == accountId)
                    .Select(a => new
                    {
                        StudentName = a.FName + " " + a.LName,
                        StudentId = a.AccountID
                    })
                    .OrderBy(a => a.StudentName)
                    .ToListAsync();

                var chartData = new List<object>();
                
                foreach (var student in teacherStudents)
                {
                    var studentAttendance = await _context.Attendances
                        .Where(a => a.StudentId == student.StudentId && 
                                   a.Date >= startDate && a.Date <= endDate)
                        .GroupBy(a => a.Status)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());

                    chartData.Add(new
                    {
                        StudentName = student.StudentName,
                        Present = studentAttendance.GetValueOrDefault("Present", 0),
                        Absent = studentAttendance.GetValueOrDefault("Absent", 0),
                        Late = studentAttendance.GetValueOrDefault("Late", 0)
                    });
                }

                ViewBag.ChartData = chartData;
            }
            else
            {
                // Admin sees subject-based data (existing logic)
                var rawData = await _context.Attendances
                    .Where(a => a.Date >= startDate && a.Date <= endDate)
                    .Join(_context.Accounts, a => a.StudentId, acc => acc.AccountID, (a, acc) => new { a, acc })
                    .Join(_context.TeacherSections, x => x.acc.SectionID, ts => ts.SectionID, (x, ts) => new { x.a, x.acc, ts })
                    .Join(_context.Accounts, x => x.ts.TeacherID, teacher => teacher.AccountID, (x, teacher) => new { x.a, x.acc, x.ts, teacher })
                    .Join(_context.Sections, x => x.ts.SectionID, s => s.SectionID, (x, s) => new { x.a, x.acc, x.ts, x.teacher, s })
                    .GroupBy(x => new { x.s.SectionName, TeacherName = x.teacher.FName + " " + x.teacher.LName })
                    .Select(g => new
                    {
                        Subject = g.Key.SectionName,
                        Teacher = g.Key.TeacherName,
                        Present = g.Count(x => x.a.Status == "Present"),
                        Absent = g.Count(x => x.a.Status == "Absent"),
                        Late = g.Count(x => x.a.Status == "Late")
                    })
                    .OrderBy(x => x.Subject)
                    .ToListAsync();

                // FORMAT IN MEMORY
                var chartData = rawData.Select(x => new
                {
                    Subject = x.Subject,
                    x.Present,
                    x.Absent,
                    x.Late
                }).ToList();

                ViewBag.ChartData = chartData;
            }
            ViewBag.ViewType = view;

            return View();
        }
    }
}
