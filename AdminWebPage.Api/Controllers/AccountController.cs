using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AdminWebPageContext _context;

        public AccountController(AdminWebPageContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.Now });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {   
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Username and password are required");
            }
            
            // Trim input to prevent issues with spaces
            var username = loginRequest.Username.Trim();
            var password = loginRequest.Password.Trim();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (account == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var response = new
            {
                AccountID = account.AccountID,
                Username = account.Username,
                Role = account.Role,
                FName = account.FName,
                LName = account.LName,
                Email = account.Email,
                TeacherID = account.TeacherID
            };

            return Ok(response);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
