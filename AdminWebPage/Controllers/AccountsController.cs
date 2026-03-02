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
        public async Task<IActionResult> Index(string search, string role, int? sectionId, int page = 1)
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
            var accounts = _context.Accounts.AsQueryable();

            // Filter based on user role
            if (userRole == "Teacher")
            {
                // Teachers can only see students assigned to them
                accounts = accounts.Where(a => a.TeacherID == accountId);
                
                // Get teacher's assigned sections for dropdown
                var teacherSections = await _context.TeacherSections
                    .Where(ts => ts.TeacherID == accountId)
                    .Include(ts => ts.Section)
                    .ToListAsync();
                
                ViewBag.TeacherSections = teacherSections;
                
                // If a section is selected, filter students by that section
                if (sectionId.HasValue)
                {
                    accounts = accounts.Where(a => a.SectionID == sectionId.Value);
                }
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
            ViewBag.SectionId = sectionId;

            return View(pagedAccounts);
        }



        // GET: Accounts/Search
        //   public async Task<IActionResult> Search(string search, string role)
        //   {
        //       var accounts = _context.Accounts.AsQueryable();

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

            var account = await _context.Accounts
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
        public async Task<IActionResult> Create([Bind("AccountID,FName,MName,LName,Username,Email,Password,Role,TeacherID,SectionID")] Account account, int? selectedTeacherId = null, int? selectedSectionId = null, List<int> selectedSectionIds = null)
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
                // Assign section ID if provided
                account.SectionID = selectedSectionId;
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
                    account.TeacherID = selectedTeacherId;
                    account.SectionID = selectedSectionId;
                }
            }
            else if (userRole == "Admin")
            {
                // Admin can create any role (no restrictions)
                if (account.Role == "Student")
                {
                    // Admin creating student can assign to teacher and section
                    account.TeacherID = selectedTeacherId;
                    account.SectionID = selectedSectionId;
                }
                else if (account.Role == "Teacher")
                {
                    // Admin creating teacher - handle section assignments after saving account
                    account.TeacherID = null;
                    account.SectionID = null;
                }
            }

            if (ModelState.IsValid)
            {
                // Save the account first to get the AccountID
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                
                // Now handle teacher-section assignments if creating a teacher
                if (account.Role == "Teacher" && selectedSectionIds != null && selectedSectionIds.Any())
                {
                    // Create teacher-section assignments using the newly saved AccountID
                    var teacherSectionAssignments = selectedSectionIds.Select(sectionId => new TeacherSection
                    {
                        TeacherID = account.AccountID, // Use the AccountID from the saved account
                        SectionID = sectionId,
                        AssignedAt = DateTime.Now
                    }).ToList();
                    
                    _context.TeacherSections.AddRange(teacherSectionAssignments);
                    await _context.SaveChangesAsync();
                }
                
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

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("AccountID,FName,MName,LName,Username,Email,Password,Role,TeacherID,SectionID")] Account account, int? selectedTeacherId = null, int? selectedSectionId = null, List<int> selectedSectionIds = null)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var accountId = HttpContext.Session.GetInt32("AccountID");
            
            // Retrieve the existing account to avoid tracking conflicts
            var existingAccount = await _context.Accounts.FindAsync(id);
            if (existingAccount == null)
            {
                return NotFound();
            }
            
            // Check permissions
            if (userRole == "Teacher")
            {
                // Teachers can only edit students assigned to them
                if (existingAccount.TeacherID != accountId)
                {
                    TempData["Error"] = "Access Denied: You can only edit students assigned to you.";
                    return RedirectToAction(nameof(Index));
                }
                // Teachers cannot change to role
                existingAccount.FName = account.FName;
                existingAccount.MName = account.MName;
                existingAccount.LName = account.LName;
                existingAccount.Username = account.Username;
                existingAccount.Email = account.Email;
                existingAccount.Password = account.Password;
                existingAccount.Role = existingAccount.Role; // Keep original role
                existingAccount.TeacherID = accountId;
                existingAccount.SectionID = selectedSectionId;
            }
            else if (userRole == "Student")
            {
                // Students have full access to edit any account
                existingAccount.FName = account.FName;
                existingAccount.MName = account.MName;
                existingAccount.LName = account.LName;
                existingAccount.Username = account.Username;
                existingAccount.Email = account.Email;
                existingAccount.Password = account.Password;
                existingAccount.Role = account.Role;
                existingAccount.TeacherID = (account.Role == "Student") ? selectedTeacherId : null;
                existingAccount.SectionID = (account.Role == "Student") ? selectedSectionId : null;
                
                if (account.Role == "Teacher")
                {
                    existingAccount.TeacherID = null;
                    existingAccount.SectionID = null;
                    
                    // Update teacher-section assignments
                    if (selectedSectionIds != null && selectedSectionIds.Any())
                    {
                        // Remove existing assignments
                        var existingAssignments = await _context.TeacherSections
                            .Where(ts => ts.TeacherID == id)
                            .ToListAsync();
                        _context.TeacherSections.RemoveRange(existingAssignments);
                        
                        // Add new assignments
                        var teacherSectionAssignments = selectedSectionIds.Select(sectionId => new TeacherSection
                        {
                            TeacherID = id,
                            SectionID = sectionId,
                            AssignedAt = DateTime.Now
                        }).ToList();
                        
                        _context.TeacherSections.AddRange(teacherSectionAssignments);
                    }
                    else
                    {
                        // Remove all assignments if none selected
                        var existingAssignments = await _context.TeacherSections
                            .Where(ts => ts.TeacherID == id)
                            .ToListAsync();
                        _context.TeacherSections.RemoveRange(existingAssignments);
                    }
                }
            }
            else if (userRole == "Admin")
            {
                // Admin can edit any account
                existingAccount.FName = account.FName;
                existingAccount.MName = account.MName;
                existingAccount.LName = account.LName;
                existingAccount.Username = account.Username;
                existingAccount.Email = account.Email;
                existingAccount.Password = account.Password;
                existingAccount.Role = account.Role;
                
                if (account.Role == "Student")
                {
                    // Admin editing student can assign to teacher and section
                    existingAccount.TeacherID = selectedTeacherId;
                    existingAccount.SectionID = selectedSectionId;
                }
                else if (account.Role == "Teacher")
                {
                    // Admin editing teacher - handle section assignments
                    existingAccount.TeacherID = null;
                    existingAccount.SectionID = null;
                    
                    // Update teacher-section assignments
                    if (selectedSectionIds != null && selectedSectionIds.Any())
                    {
                        // Remove existing assignments
                        var existingAssignments = await _context.TeacherSections
                            .Where(ts => ts.TeacherID == id)
                            .ToListAsync();
                        _context.TeacherSections.RemoveRange(existingAssignments);
                        
                        // Add new assignments
                        var teacherSectionAssignments = selectedSectionIds.Select(sectionId => new TeacherSection
                        {
                            TeacherID = id,
                            SectionID = sectionId,
                            AssignedAt = DateTime.Now
                        }).ToList();
                        
                        _context.TeacherSections.AddRange(teacherSectionAssignments);
                    }
                    else
                    {
                        // Remove all assignments if none selected
                        var existingAssignments = await _context.TeacherSections
                            .Where(ts => ts.TeacherID == id)
                            .ToListAsync();
                        _context.TeacherSections.RemoveRange(existingAssignments);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(existingAccount.AccountID))
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
            return View(existingAccount);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
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
            
            var account = await _context.Accounts.FindAsync(id);
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
                
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountID == id);
        }

        private async Task<string> GenerateStudentUsername()
        {
            string prefix = "26-";

            // Get last student username
            var lastUsername = await _context.Accounts
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
            while (await _context.Accounts.AnyAsync(a => a.Username == newUsername));

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
