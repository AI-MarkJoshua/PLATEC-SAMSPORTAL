using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentMobile.Models
{
    public class AttendanceDto
    {
        public int StudentId { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
}
