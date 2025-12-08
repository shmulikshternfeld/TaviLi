using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaviLi.Domain.Entities;
using TaviLi.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace TaviLi.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure DB is created
            await context.Database.EnsureCreatedAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Users
            var clientUser = await SeedUserAsync(userManager, "client@test.com", "Client", "דני לקוח");
            var courierUser = await SeedUserAsync(userManager, "courier@test.com", "Courier", "יוסי שליח");

            // Seed Missions
            if (!context.Missions.Any())
            {
                context.Missions.AddRange(
                    new Mission
                    {
                        CreatorUserId = clientUser.Id,
                        PickupAddress = "רחוב הרצל 1, תל אביב",
                        DropoffAddress = "רחוב ויצמן 10, רחובות",
                        PackageSize = PackageSize.Medium,
                        OfferedPrice = 150,
                        Status = MissionStatus.Open,
                        CreationTime = DateTime.Now.AddHours(-2),
                        PackageDescription = "חבילת ספרים שבירה"
                    },
                    new Mission
                    {
                        CreatorUserId = clientUser.Id,
                        PickupAddress = "בן יהודה 20, תל אביב",
                        DropoffAddress = "הירקון 5, חיפה",
                        PackageSize = PackageSize.Small,
                        OfferedPrice = 80,
                        Status = MissionStatus.Open,
                        CreationTime = DateTime.Now.AddHours(-5),
                        PackageDescription = "מעטפה דחופה"
                    }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            string[] roleNames = { "Client", "Courier" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new Role { Name = roleName });
                }
            }
        }

        private static async Task<User> SeedUserAsync(UserManager<User> userManager, string email, string role, string fullName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    Name = fullName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }
    }
}
