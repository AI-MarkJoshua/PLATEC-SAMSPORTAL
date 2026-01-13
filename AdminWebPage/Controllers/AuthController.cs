using AdminWebPage.Data;
using AdminWebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Controllers
{
    public class AuthController : Controller
    {
        private readonly AdminWebPageContext _context;

        public AuthController(AdminWebPageContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.Account
                    .FirstOrDefaultAsync(a =>
                        a.Email == model.Email &&
                        a.Password == model.Password);

                if (account != null)
                {
                    if (account.Role == "Teacher")
                    {
                        // Teacher login → allow access
                        HttpContext.Session.SetString("UserEmail", account.Email);
                        HttpContext.Session.SetString("UserRole", account.Role);
                        return RedirectToAction("Index", "Accounts");
                    }
                    else if (account.Role == "Student")
                    {
                        // Student login → restricted access
                        ViewBag.Error = "Access Restricted: Students cannot log in to this page.";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Error = "Invalid email or password";
                }
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}