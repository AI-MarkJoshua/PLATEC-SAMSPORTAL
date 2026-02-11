using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AdminWebPage.Shared.Models;

namespace AdminWebPage.Shared.Data
{
    public class AdminWebPageContext : DbContext
    {
        public AdminWebPageContext(DbContextOptions<AdminWebPageContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Account { get; set; } = default!;
        public DbSet<Attendance> Attendances { get; set; } = default!;
    }
}

