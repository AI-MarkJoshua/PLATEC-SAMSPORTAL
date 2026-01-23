using AdminWebPage.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AdminWebPageContext _context;

        public DashboardController(AdminWebPageContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string view = "daily")
        {
            // 🔢 COUNTS
            ViewBag.TotalStudents = await _context.Account
                .CountAsync(a => a.Role == "Student");

            ViewBag.TotalTeachers = await _context.Account
                .CountAsync(a => a.Role == "Teacher");

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

            // 📅 DATE RANGE
            DateTime startDate;
            DateTime endDate = DateTime.Today;

            if (view == "weekly")
                startDate = DateTime.Today.AddDays(-6);
            else
                startDate = DateTime.Today;

            // 📊 ATTENDANCE CHART DATA
            var rawData = await _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .GroupBy(a => a.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Present = g.Count(x => x.Status == "Present"),
                    Absent = g.Count(x => x.Status == "Absent"),
                    Late = g.Count(x => x.Status == "Late")
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // 📊 FORMAT IN MEMORY
            var chartData = rawData.Select(x => new
            {
                Date = x.Date.ToString("MMM dd"),
                x.Present,
                x.Absent,
                x.Late
            }).ToList();

            ViewBag.ChartData = chartData;
            ViewBag.ViewType = view;

            return View();
        }
    }
}
