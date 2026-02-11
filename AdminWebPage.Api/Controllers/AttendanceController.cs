using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AdminWebPageContext _context;

        public AttendanceController(AdminWebPageContext context)
        {
            _context = context;
        }

        // GET: api/attendance/students
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Account
                .Where(a => a.Role == "Student")
                .Select(a => new
                {
                    a.AccountID,
                    a.FName,
                    a.MName,
                    a.LName
                })
                .ToListAsync();

            return Ok(students);
        }

        // POST: api/attendance/mark
        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceDto dto)
        {
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.StudentId == dto.StudentId &&
                    a.Date == dto.Date.Date);

            if (existing != null)
            {
                existing.Status = dto.Status;
            }
            else
            {
                _context.Attendances.Add(new Attendance
                {
                    StudentId = dto.StudentId,
                    Date = dto.Date.Date,
                    Status = dto.Status
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/attendance/markall
        [HttpPost("markall")]
        public async Task<IActionResult> MarkAll([FromBody] List<AttendanceDto> attendances)
        {
            if (attendances == null || attendances.Count == 0)
                return BadRequest("Attendance list is required");

            foreach (var dto in attendances)
            {
                // check if record exists for this student and date
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a =>
                        a.StudentId == dto.StudentId &&
                        a.Date.Date == dto.Date.Date);

                if (existing != null)
                {
                    existing.Status = dto.Status; // update existing
                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        StudentId = dto.StudentId,
                        Date = dto.Date.Date,
                        Status = dto.Status
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(attendances); // return the list for confirmation
        }

    }
}
