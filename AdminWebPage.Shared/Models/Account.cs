using System.ComponentModel.DataAnnotations;

namespace AdminWebPage.Shared.Models
{
    public class Account
    {
        public int AccountID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;   // ✅ NEW (for login)

        [Required]
        [StringLength(50)]
        public string FName { get; set; } = string.Empty;

        public string? MName { get; set; }

        [Required]
        [StringLength(50)]
        public string LName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;      // ✅ Used for forgot password

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty;      // Admin / Teacher / Student

        // ✅ NEW: For student-teacher assignment (nullable for Admin/Teacher)
        public int? TeacherID { get; set; }
        public virtual Account? Teacher { get; set; }

        // ✅ NEW: For student section assignment (nullable for Admin/Teacher)
        public int? SectionID { get; set; }
        public virtual Section? Section { get; set; }

        // ✅ NEW: For teacher-section assignments (only for teachers)
        public virtual ICollection<TeacherSection> TeacherSections { get; set; } = new List<TeacherSection>();
    }
}
