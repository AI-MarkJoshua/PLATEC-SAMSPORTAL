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
                new Section { SectionName = "Math Subject" },
                new Section { SectionName = "Filipino Subject" },
                new Section { SectionName = "English" },
                new Section { SectionName = "Science" },
                new Section { SectionName = "History" }
            };

            await context.Sections.AddRangeAsync(sections);
            await context.SaveChangesAsync();
        }
    }
}
