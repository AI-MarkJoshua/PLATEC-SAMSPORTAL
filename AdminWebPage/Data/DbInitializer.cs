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
                // Seed sections first
                await SectionSeeder.SeedSectionsAsync(context);

                // Check if admin user already exists
                var existingAdmin = await context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == "admin");

                if (existingAdmin == null)
                {
                    // Create default admin account
                    var admin = new Account
                    {
                        Username = "admin",
                        FName = "System",
                        MName = null,
                        LName = "Administrator",
                        Email = "admin@system.com",
                        Password = "admin123",
                        Role = "Admin",
                        TeacherID = null,
                        SectionID = null
                    };

                    context.Accounts.Add(admin);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
                throw;
            }
        }
    }
}
