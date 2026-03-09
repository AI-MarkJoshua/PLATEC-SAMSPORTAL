using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentMobile.Models
{
    public class AttendanceRecord
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } // Present, Absent, Late
        public string Subject { get; set; } // Subject name
        public string TeacherName { get; set; } // Teacher name
        
        // Computed properties for UI convenience
        public string StatusDisplay => Status?.ToUpper() ?? "UNKNOWN";
        public string DateDisplay => Date.ToString("MMM dd, yyyy");
        public string TimeDisplay => Date.ToString("hh:mm tt");
    }
}
