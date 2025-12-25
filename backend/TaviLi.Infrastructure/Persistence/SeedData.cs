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
                        PickupAddress = new TaviLi.Domain.ValueObjects.Address 
                        { 
                            FullAddress = "רחוב הרצל 1, תל אביב",
                            Location = new NetTopologySuite.Geometries.Point(34.7818, 32.0853) { SRID = 4326 },
                            City = "תל אביב",
                            Street = "הרצל",
                            HouseNumber = "1"
                        },
                        DropoffAddress = new TaviLi.Domain.ValueObjects.Address 
                        { 
                            FullAddress = "רחוב ויצמן 10, רחובות",
                            Location = new NetTopologySuite.Geometries.Point(34.8113, 31.8928) { SRID = 4326 },
                            City = "רחובות",
                            Street = "ויצמן",
                            HouseNumber = "10"
                        },
                        PackageSize = PackageSize.Medium,
                        OfferedPrice = 150,
                        Status = MissionStatus.Open,
                        CreationTime = DateTime.Now.AddHours(-2),
                        PackageDescription = "חבילת ספרים שבירה"
                    },
                    new Mission
                    {
                        CreatorUserId = clientUser.Id,
                        PickupAddress = new TaviLi.Domain.ValueObjects.Address 
                        { 
                            FullAddress = "בן יהודה 20, תל אביב",
                            Location = new NetTopologySuite.Geometries.Point(34.7818, 32.0853) { SRID = 4326 },
                            City = "תל אביב",
                            Street = "בן יהודה",
                            HouseNumber = "20"
                        },
                        DropoffAddress = new TaviLi.Domain.ValueObjects.Address 
                        { 
                            FullAddress = "הירקון 5, חיפה",
                            Location = new NetTopologySuite.Geometries.Point(34.9896, 32.7940) { SRID = 4326 },
                            City = "חיפה",
                            Street = "הירקון",
                            HouseNumber = "5"
                        },
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
