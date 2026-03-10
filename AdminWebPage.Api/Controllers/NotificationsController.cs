using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AdminWebPageContext _context;

        public NotificationsController(AdminWebPageContext context)
        {
            _context = context;
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentNotifications(int studentId)
        {
            try
            {
                var notifications = await GenerateNotificationsForStudent(studentId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                // In a real implementation, you would have a Notifications table
                // For now, we'll just return success
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task<List<object>> GenerateNotificationsForStudent(int studentId)
        {
            var notifications = new List<object>();
            var today = DateTime.Today;

            // Check for recent absences
            var recentAbsences = await _context.Attendances
                .Where(a => a.StudentId == studentId && 
                           a.Status == "Absent" && 
                           a.Date >= today.AddDays(-7))
                .Include(a => a.Student)
                .ThenInclude(s => s.Section)
                .ToListAsync();

            foreach (var absence in recentAbsences)
            {
                notifications.Add(new
                {
                    Id = 1000 + absence.AttendanceId,
                    StudentId = studentId,
                    Title = "Absence Alert",
                    Message = $"You were marked absent in {absence.Student?.Section?.SectionName} on {absence.Date:MMM dd, yyyy}",
                    Type = "Absent",
                    CreatedAt = absence.Date,
                    IsRead = false
                });
            }

            // Check for recent late marks
            var recentLates = await _context.Attendances
                .Where(a => a.StudentId == studentId && 
                           a.Status == "Late" && 
                           a.Date >= today.AddDays(-7))
                .Include(a => a.Student)
                .ThenInclude(s => s.Section)
                .ToListAsync();

            foreach (var late in recentLates)
            {
                notifications.Add(new
                {
                    Id = 2000 + late.AttendanceId,
                    StudentId = studentId,
                    Title = "Late Alert",
                    Message = $"You were marked late in {late.Student?.Section?.SectionName} on {late.Date:MMM dd, yyyy}",
                    Type = "Late",
                    CreatedAt = late.Date,
                    IsRead = false
                });
            }

            // Check for consecutive absences
            var consecutiveAbsences = await _context.Attendances
                .Where(a => a.StudentId == studentId && 
                           a.Status == "Absent" && 
                           a.Date >= today.AddDays(-3))
                .OrderByDescending(a => a.Date)
                .Take(3)
                .ToListAsync();

            if (consecutiveAbsences.Count >= 3)
            {
                notifications.Add(new
                {
                    Id = 3000,
                    StudentId = studentId,
                    Title = "Attendance Warning",
                    Message = "You have been absent for 3 consecutive days. Please contact your teacher.",
                    Type = "Absent",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            // Check for good attendance
            var presentThisWeek = await _context.Attendances
                .Where(a => a.StudentId == studentId && 
                           a.Status == "Present" && 
                           a.Date >= today.AddDays(-7))
                .CountAsync();

            if (presentThisWeek >= 5)
            {
                notifications.Add(new
                {
                    Id = 4000,
                    StudentId = studentId,
                    Title = "Good Attendance!",
                    Message = $"Great job! You've been present {presentThisWeek} times this week.",
                    Type = "Present",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            return notifications.OrderByDescending(n => ((dynamic)n).CreatedAt).ToList();
        }
    }
}
