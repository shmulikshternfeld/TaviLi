using Microsoft.AspNetCore.Identity;
using TaviLi.Domain.Entities;

namespace TaviLi.Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            // רשימת התפקידים שאנו רוצים במערכת
            string[] roleNames = { "Client", "Courier" };

            foreach (var roleName in roleNames)
            {
                // בדוק אם התפקיד כבר קיים
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // אם לא קיים, צור אותו
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }
    }
}