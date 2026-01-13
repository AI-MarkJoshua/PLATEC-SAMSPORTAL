namespace AdminWebPage.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public string FName { get; set; }
        public string MName { get; set; }
        public string LName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }  // "Teacher" or "Student"

    }
}
