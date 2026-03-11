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

        // GET: api/attendance/mine/{studentId}
        [HttpGet("mine/{studentId}")]
        public async Task<IActionResult> GetMyAttendance(int studentId)
        {
            var attendance = await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .Join(_context.Accounts, 
                    a => a.StudentId, 
                    s => s.AccountID, 
                    (a, s) => new { a, s })
                .Join(_context.Accounts, 
                    as_join => as_join.s.TeacherID, 
                    t => t.AccountID, 
                    (as_join, t) => new { as_join.a, Student = as_join.s, Teacher = t })
                .Join(_context.Sections,
                    ats => ats.Student.SectionID,
                    sec => sec.SectionID,
                    (ats, sec) => new { ats.a, ats.Student, ats.Teacher, Section = sec })
                .Select(x => new
                {
                    x.a.Date,
                    x.a.Status,
                    Subject = x.Section.SectionName,
                    TeacherName = x.Teacher.FName + " " + x.Teacher.LName
                })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            return Ok(attendance);
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
