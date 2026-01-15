namespace AdminWebPage.Models
{
    public class Account
    {
        public int AccountID { get; set; }

        public string Username { get; set; }   // ✅ NEW (for login)

        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }

        public string Email { get; set; }      // ✅ Used for forgot password
        public string Password { get; set; }

        public string Role { get; set; }  // Teacher / Student
    }
}
