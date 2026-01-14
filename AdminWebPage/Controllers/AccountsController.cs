using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminWebPage.Data;
using AdminWebPage.Models;

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
        public async Task<IActionResult> Index(string search, string role)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                TempData["Error"] = "Access Denied: Only teachers can access this page.";
                return RedirectToAction("Login", "Auth");
            }

            var accounts = _context.Account.AsQueryable();

            // 🔍 Search by name (FName, MName, LName)
            if (!string.IsNullOrEmpty(search))
            {
                accounts = accounts.Where(a =>
                    a.FName.Contains(search) ||
                    a.MName.Contains(search) ||
                    a.LName.Contains(search));
            }

            // 🎯 Filter by role
            if (!string.IsNullOrEmpty(role))
            {
                accounts = accounts.Where(a => a.Role == role);
            }

            ViewBag.Search = search;
            ViewBag.Role = role;

            return View(await accounts.ToListAsync());
        }

        // GET: Accounts/Search
        public async Task<IActionResult> Search(string search, string role)
        {
            var accounts = _context.Account.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                accounts = accounts.Where(a =>
                    a.FName.Contains(search) ||
                    a.MName.Contains(search) ||
                    a.LName.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                accounts = accounts.Where(a => a.Role == role);
            }

            var result = await accounts.Select(a => new
            {
                a.AccountID,
                a.FName,
                a.MName,
                a.LName,
                a.Email,
                a.Role
            }).ToListAsync();

            return Json(result);
        }



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
        public async Task<IActionResult> Create([Bind("AccountID,FName,MName,LName,Email,Password,Role")] Account account)
        {
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
        public async Task<IActionResult> Edit(int id, [Bind("AccountID,FName,MName,LName,Email,Password,Role")] Account account)
        {
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
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.AccountID == id);
        }
    }
}
