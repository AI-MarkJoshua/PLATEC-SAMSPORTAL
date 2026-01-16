using AdminWebPage.Data;
using AdminWebPage.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

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
     .AsNoTracking()
     .FirstOrDefaultAsync(a =>
         a.Username == model.Username &&
         a.Password == model.Password);


                if (account != null)
                {
                    if (account.Role == "Teacher")
                    {
                        HttpContext.Session.SetString("UserRole", account.Role);
                        HttpContext.Session.SetString("Username", account.Username);

                        return RedirectToAction("Index", "Accounts");
                    }
                    else
                    {
                        ViewBag.Error = "Access Restricted: Students cannot log in.";
                    }
                }
                else
                {
                    ViewBag.Error = "Invalid username or password";
                }
            }

            return View(model);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var account = await _context.Account.FirstOrDefaultAsync(a => a.Email == email);

            if (account == null)
            {
                ViewBag.Error = "Email not found.";
                return View();
            }

            string code = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", email);

            try
            {
                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(
                        "babala123h@gmail.com",
                        "adax jrum inbk vhvp"
                    ),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress("babala123h@gmail.com"),
                    Subject = "Password Reset Code",
                    Body = $"Your verification code is: {code}"
                };

                mail.To.Add(email);
                await smtp.SendMailAsync(mail);
            }
            catch
            {
                ViewBag.Error = "Failed to send email.";
                return View();
            }

            return RedirectToAction("VerifyCode");
        }

        public IActionResult VerifyCode()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyCode(string code)
        {
            string savedCode = HttpContext.Session.GetString("ResetCode");

            if (code != savedCode)
            {
                ViewBag.Error = "Invalid verification code.";
                return View();
            }

            return RedirectToAction("ResetPassword");
        }



        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            string email = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Session expired. Please try again.";
                return View();
            }

            var account = await _context.Account.FirstOrDefaultAsync(a => a.Email == email);

            if (account == null)
            {
                ViewBag.Error = "Account not found.";
                return View();
            }

            // ✅ UPDATE PASSWORD
            account.Password = newPassword;

            // ✅ FORCE EF TO TRACK THE CHANGE
            _context.Account.Update(account);
            await _context.SaveChangesAsync();

            // ✅ CLEAR SESSION
            HttpContext.Session.Clear();

            TempData["Success"] = "Password reset successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}