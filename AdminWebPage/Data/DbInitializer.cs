using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(AdminWebPageContext context)
        {
            try
            {
                // Try to apply migrations first
                await context.Database.MigrateAsync();
            }
            catch
            {
                // If migrations fail, ensure database is created
                await context.Database.EnsureCreatedAsync();
            }

            // Check if any admin account exists
            var adminExists = await context.Account.AnyAsync(a => a.Role == "Admin");
            
            if (!adminExists)
            {
                // Create default admin account
                var admin = new Account
                {
                    FName = "System",
                    MName = "",
                    LName = "Administrator",
                    Username = "admin",
                    Email = "admin@sams.com",
                    Password = "admin123",
                    Role = "Admin"
                    // Note: TeacherID will be null by default, don't set it explicitly
                    // to avoid issues if the column doesn't exist yet
                };

                try
                {
                    context.Account.Add(admin);
                    await context.SaveChangesAsync();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    // If TeacherID column doesn't exist, we need to apply migration manually
                    // For now, let's create the admin without the TeacherID property
                    Console.WriteLine("Database migration needed. Please run: dotnet-ef database update");
                    throw new Exception("Database needs to be updated. Please stop the application and run 'dotnet-ef database update' in the AdminWebPage folder.", ex);
                }
            }
        }
    }
}
