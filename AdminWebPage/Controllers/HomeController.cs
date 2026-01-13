using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AdminWebPage.Models;
using Microsoft.EntityFrameworkCore;
using AdminWebPage.Data;

namespace AdminWebPage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AdminWebPageContext _context;

        public HomeController(ILogger<HomeController> logger, AdminWebPageContext context)
        {
            _logger = logger;
            _context = context;

            // Create tables if not exists (including Accounts)
            ExistingTableDB();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void ExistingTableDB()
        {
            // Accounts table
            string accountQuery = @"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Account')
            BEGIN
                CREATE TABLE [dbo].[Account] (
                    [AccountID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [FName] NVARCHAR(50) NOT NULL,
                    [MName] NVARCHAR(50) NULL,
                    [LName] NVARCHAR(50) NOT NULL,
                    [Email] NVARCHAR(100) NOT NULL,
                    [Password] NVARCHAR(100) NOT NULL,
                    [Role] NVARCHAR(20) NOT NULL
                );
            END";

            _context.Database.ExecuteSqlRaw(accountQuery);

            string attendanceQuery = @"
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Attendances')
    BEGIN
        CREATE TABLE [dbo].[Attendances] (
            [AttendanceId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            [StudentId] INT NOT NULL,
            [Date] DATE NOT NULL,
            [Status] NVARCHAR(20) NOT NULL, -- Present, Absent, Late
            CONSTRAINT FK_Attendance_Student FOREIGN KEY (StudentId) REFERENCES Account(AccountID)
        );
    END";

            _context.Database.ExecuteSqlRaw(attendanceQuery);
        }
    }
}
