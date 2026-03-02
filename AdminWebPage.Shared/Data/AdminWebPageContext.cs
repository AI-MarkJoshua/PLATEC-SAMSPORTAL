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

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<TeacherSection> TeacherSections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Account entity
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountID);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                
                // Self-referencing relationship for TeacherID
                entity.HasOne(a => a.Teacher)
                      .WithMany()
                      .HasForeignKey(a => a.TeacherID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Section entity
            modelBuilder.Entity<Section>(entity =>
            {
                entity.HasKey(e => e.SectionID);
                entity.Property(e => e.SectionName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure TeacherSection entity
            modelBuilder.Entity<TeacherSection>(entity =>
            {
                entity.HasKey(e => e.TeacherSectionID);
                
                entity.HasOne(e => e.Teacher)
                      .WithMany(a => a.TeacherSections)
                      .HasForeignKey(e => e.TeacherID)
                      .OnDelete(DeleteBehavior.Cascade);
                      

                entity.HasOne(e => e.Section)
                      .WithMany(s => s.TeacherSections)
                      .HasForeignKey(e => e.SectionID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
