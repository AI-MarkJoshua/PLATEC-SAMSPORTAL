using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;


namespace AdminWebPage.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AdminWebPageContext _context;

        public AccountsController(AdminWebPageContext context)
        {
            _context = context;
        }

        // GET: Accounts
        // GET: Accounts
        public async Task<IActionResult> Index(string search, string role, int page = 1)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Only Admin, Teacher, and Student can access this page
            if (userRole != "Admin" && userRole != "Teacher" && userRole != "Student")
            {
                TempData["Error"] = "Access Denied.";
                return RedirectToAction("Login", "Auth");
            }

            int pageSize = 10;
            var accounts = _context.Account.AsQueryable();

            // Filter based on user role
            if (userRole == "Teacher")
            {
                // Teachers can only see students assigned to them
                accounts = accounts.Where(a => a.TeacherID == accountId);
            }
            // Admin and Student can see all accounts

            // 🔍 Search
            if (!string.IsNullOrEmpty(search))
            {
                accounts = accounts.Where(a =>
                    a.FName.Contains(search) ||
                    (a.MName != null && a.MName.Contains(search)) ||
                    a.LName.Contains(search));
            }

            // 🎯 Filter
            if (!string.IsNullOrEmpty(role))
            {
                accounts = accounts.Where(a => a.Role == role);
            }

            int totalRecords = await accounts.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // ✅ Safety: prevent invalid page
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedAccounts = await accounts
                .OrderBy(a => a.AccountID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(pagedAccounts);
        }



        // GET: Accounts/Search
        //   public async Task<IActionResult> Search(string search, string role)
        //   {
        //       var accounts = _context.Account.AsQueryable();

        //       if (!string.IsNullOrEmpty(search))
        //       {
        //           accounts = accounts.Where(a =>
        //a.FName.Contains(search) ||
        //(a.MName != null && a.MName.Contains(search)) ||
        //a.LName.Contains(search));

        //       }

        //       if (!string.IsNullOrEmpty(role))
        //       {
        //           accounts = accounts.Where(a => a.Role == role);
        //       }

        //       var result = await accounts.Select(a => new
        //       {
        //           a.AccountID,
        //           a.FName,
        //           a.MName,
        //           a.LName,
        //           a.Username,
        //           a.Email,
        //           a.Role
        //       }).ToListAsync();

        //       return Json(result);
        //   }



        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.AccountID == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountID,FName,MName,LName,Username,Email,Password,Role,TeacherID")] Account account, int? selectedTeacherId = null)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions based on user role
            if (userRole == "Teacher")
            {
                // Teachers can only create students
                if (account.Role != "Student")
                {
                    ModelState.AddModelError("Role", "Teachers can only create student accounts.");
                    return View(account);
                }
                // Assign the teacher ID to the student
                account.TeacherID = accountId;
            }
            else if (userRole == "Student")
            {
                // Students have super user access - can create admin, teacher, and student accounts
                if (account.Role != "Admin" && account.Role != "Teacher" && account.Role != "Student")
                {
                    ModelState.AddModelError("Role", "Invalid role selected.");
                    return View(account);
                }
                // Students don't assign TeacherID (null for admin/teacher, will be set for students)
                if (account.Role == "Student")
                {
                    // If student creates another student, you might want to assign them to a teacher
                    // For now, leave TeacherID null - you can modify this logic as needed
                    account.TeacherID = selectedTeacherId;
                }
            }
            else if (userRole == "Admin")
            {
                // Admin can create any role (no restrictions)
                if (account.Role == "Student")
                {
                    // Admin creating student can assign to teacher
                    account.TeacherID = selectedTeacherId;
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountID,FName,MName,LName,Username,Email,Password,Role,TeacherID")] Account account)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Check permissions
            if (userRole == "Teacher")
            {
                // Teachers can only edit students assigned to them
                var targetAccount = await _context.Account.FindAsync(id);
                if (targetAccount == null || targetAccount.TeacherID != accountId)
                {
                    TempData["Error"] = "Access Denied: You can only edit students assigned to you.";
                    return RedirectToAction(nameof(Index));
                }
                // Teachers cannot change the role
                account.Role = targetAccount.Role;
                account.TeacherID = accountId;
            }
            else if (userRole == "Student")
            {
                // Students have full access - can edit any account
                // No restrictions for students
            }
            // Admin can edit any account

            if (id != account.AccountID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.AccountID == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                // Check permissions
                if (userRole == "Teacher")
                {
                    // Teachers can only delete students assigned to them
                    if (account.TeacherID != accountId)
                    {
                        TempData["Error"] = "Access Denied: You can only delete students assigned to you.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                // Students and Admins can delete any account
                
                _context.Account.Remove(account);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.AccountID == id);
        }

        private async Task<string> GenerateStudentUsername()
        {
            string prefix = "26-";

            // Get last student username
            var lastUsername = await _context.Account
                .Where(a => a.Role == "Student" && a.Username.StartsWith(prefix))
                .OrderByDescending(a => a.Username)
                .Select(a => a.Username)
                .FirstOrDefaultAsync();

            int nextNumber = 2000001;

            if (!string.IsNullOrEmpty(lastUsername))
            {
                // Extract numeric part
                var numberPart = lastUsername.Replace(prefix, "");
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string newUsername;

            do
            {
                newUsername = $"{prefix}{nextNumber}";
                nextNumber++;
            }
            while (await _context.Account.AnyAsync(a => a.Username == newUsername));

            return newUsername;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateStudentAccount()
        {
            var username = await GenerateStudentUsername();

            return Json(new
            {
                username = username,
                password = username
            });
        }

    }
}
