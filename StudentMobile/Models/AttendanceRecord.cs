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
    }
}
