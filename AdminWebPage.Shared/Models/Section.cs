using System.ComponentModel.DataAnnotations;

namespace AdminWebPage.Shared.Models
{
    public class Section
    {
        public int SectionID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SectionName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property for teachers assigned to this section
        public virtual ICollection<TeacherSection> TeacherSections { get; set; } = new List<TeacherSection>();
    }
}
