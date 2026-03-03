using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
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
        public async Task<IActionResult> Index(DateTime? selectedDate, int? selectedSectionId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions - Admin, Student, and Teacher can access
            if (userRole != "Admin" && userRole != "Student" && userRole != "Teacher")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.SelectedDate = selectedDate;
            ViewBag.SelectedSectionId = selectedSectionId;

            var studentsQuery = _context.Accounts.Where(a => a.Role == "Student");
            
            // If user is Teacher, only show students assigned to them and filter by section
            if (userRole == "Teacher")
            {
                studentsQuery = studentsQuery.Where(a => a.TeacherID == accountId);
                
                // Get teacher's assigned sections for dropdown
                var teacherSections = await _context.TeacherSections
                    .Where(ts => ts.TeacherID == accountId)
                    .Include(ts => ts.Section)
                    .ToListAsync();
                
                ViewBag.TeacherSections = teacherSections;
                
                // If a section is selected, filter students by that section
                if (selectedSectionId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(a => a.SectionID == selectedSectionId.Value);
                }
            }
            else
            {
                // For Admin and Student, get all sections
                var allSections = await _context.Sections
                    .OrderBy(s => s.SectionName)
                    .ToListAsync();
                
                ViewBag.TeacherSections = allSections.Select(s => new { SectionID = s.SectionID, Section = s }).ToList();
                
                // If a section is selected, filter students by that section
                if (selectedSectionId.HasValue)
                {
                    studentsQuery = studentsQuery.Where(a => a.SectionID == selectedSectionId.Value);
                }
            }
            
            var students = await studentsQuery.ToListAsync();

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

            // Load all attendances grouped by date and section for separate cards
            var allAttendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.Section)
                .ToListAsync();

            // Group by date and section to create separate cards for each classroom
            var attendanceByDateAndSection = allAttendance
                .GroupBy(a => new { 
                    Date = a.Date.Date, 
                    SectionName = a.Student?.Section?.SectionName ?? "Unassigned",
                    SectionId = a.Student?.SectionID ?? 0
                })
                .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.StudentId, a => a.Status));

            ViewBag.AttendanceByDateAndSection = attendanceByDateAndSection;

            // Get unique dates for filtering (still needed for some UI elements)
            var attendanceDates = allAttendance
                .Select(a => a.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            ViewBag.AttendanceDates = attendanceDates;

            return View(students);
        }





        // POST: Attendance/Mark
        [HttpPost]
        public async Task<IActionResult> Mark(int studentId, string status, DateTime date)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions - Admin and Student have full access, Teacher limited access
            if (userRole != "Admin" && userRole != "Student" && userRole != "Teacher")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Index", new { selectedDate = date });
            }
            
            // If user is Teacher, verify the student is assigned to them
            if (userRole == "Teacher")
            {
                var student = await _context.Accounts.FindAsync(studentId);
                if (student?.TeacherID != accountId)
                {
                    TempData["Error"] = "Access Denied: You can only mark attendance for students assigned to you.";
                    return RedirectToAction("Index", new { selectedDate = date });
                }
            }
            // Admin and Student have full access - no additional checks needed
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
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions - Admin and Student have full access, Teacher limited access
            if (userRole != "Admin" && userRole != "Student" && userRole != "Teacher")
            {
                return BadRequest("Access Denied.");
            }
            
            foreach (var item in attendances)
            {
                // If user is Teacher, verify the student is assigned to them
                if (userRole == "Teacher")
                {
                    var student = await _context.Accounts.FindAsync(item.StudentId);
                    if (student?.TeacherID != accountId)
                    {
                        continue; // Skip students not assigned to this teacher
                    }
                }
                // Admin and Student have full access - no additional checks needed
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
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions - Admin, Student, and Teacher can access reports
            if (userRole != "Admin" && userRole != "Student" && userRole != "Teacher")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Login", "Auth");
            }

            var reportData = new List<AttendanceReportViewModel>();
            var detailedList = new List<dynamic>();

            if (startDate == null || endDate == null)
                return View(reportData);

            var studentsQuery = _context.Accounts.Where(a => a.Role == "Student");
            
            // If user is Teacher, only show students assigned to them
            if (userRole == "Teacher")
            {
                studentsQuery = studentsQuery.Where(a => a.TeacherID == accountId);
            }
            
            var students = await studentsQuery.ToListAsync();

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
