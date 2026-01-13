using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AdminWebPage.Models;

namespace AdminWebPage.Data
{
    public class AdminWebPageContext : DbContext
    {
        public AdminWebPageContext (DbContextOptions<AdminWebPageContext> options)
            : base(options)
        {
        }

        public DbSet<AdminWebPage.Models.Account> Account { get; set; } = default!;
        public DbSet<Attendance> Attendances { get; set; }

    }
}
