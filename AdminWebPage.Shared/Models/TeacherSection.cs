using System.ComponentModel.DataAnnotations;

namespace AdminWebPage.Shared.Models
{
    public class TeacherSection
    {
        public int TeacherSectionID { get; set; }
        
        public int TeacherID { get; set; }
        public virtual Account Teacher { get; set; } = null!;
        
        public int SectionID { get; set; }
        public virtual Section Section { get; set; } = null!;
        
        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}
