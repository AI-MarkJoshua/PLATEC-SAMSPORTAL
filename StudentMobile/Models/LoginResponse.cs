using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentMobile.Models
{
    public class LoginResponse
    {
        public int AccountID { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
    }
}
