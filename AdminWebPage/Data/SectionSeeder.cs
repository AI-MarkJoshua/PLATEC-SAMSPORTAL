using AdminWebPage.Shared.Data;
using AdminWebPage.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminWebPage.Data
{
    public static class SectionSeeder
    {
        public static async Task SeedSectionsAsync(AdminWebPageContext context)
        {
            // Check if sections already exist
            if (await context.Sections.AnyAsync())
            {
                return; // Database has been seeded
            }

            var sections = new List<Section>
            {
                new Section { SectionName = "Apple" },
                new Section { SectionName = "Banana" },
                new Section { SectionName = "Grapes" },
                new Section { SectionName = "Orange" },
                new Section { SectionName = "Strawberry" }
            };

            await context.Sections.AddRangeAsync(sections);
            await context.SaveChangesAsync();
        }
    }
}
