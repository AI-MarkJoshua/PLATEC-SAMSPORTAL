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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (string.IsNullOrEmpty(model.VerificationCode))
            {
                // Step 1: Generate code and send email
                var account = await _context.Account
                    .FirstOrDefaultAsync(a => a.Email == model.Email);

                if (account == null)
                {
                    ViewBag.Error = "Email not found.";
                    return View();
                }

                // Generate 6-digit code
                var code = new Random().Next(100000, 999999).ToString();

                // Store in TempData for simplicity (in production use DB or cache)
                TempData["VerificationCode"] = code;
                TempData["UserEmail"] = model.Email;

                // Send email
                try
                {
                    var smtpClient = new SmtpClient("smtp.your-email-provider.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("your-email@example.com", "your-email-password"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("your-email@example.com"),
                        Subject = "Password Reset Verification Code",
                        Body = $"Your verification code is: {code}",
                        IsBodyHtml = false,
                    };

                    mailMessage.To.Add(model.Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Failed to send email. " + ex.Message;
                    return View();
                }

                ViewBag.Success = "Verification code sent to your email.";
                return View();
            }
            else
            {
                // Step 2: Verify code
                if (model.VerificationCode == TempData["VerificationCode"]?.ToString() &&
                    model.Email == TempData["UserEmail"]?.ToString())
                {
                    // Allow password reset
                    return RedirectToAction("ResetPassword", new { email = model.Email });
                }
                else
                {
                    ViewBag.Error = "Invalid verification code.";
                    return View();
                }
            }
        }

        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string newPassword)
        {
            var account = await _context.Account.FirstOrDefaultAsync(a => a.Email == email);

            if (account == null)
            {
                ViewBag.Error = "Email not found.";
                return View();
            }

            account.Password = newPassword;
            _context.Update(account);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password updated successfully.";
            return RedirectToAction("Login");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}