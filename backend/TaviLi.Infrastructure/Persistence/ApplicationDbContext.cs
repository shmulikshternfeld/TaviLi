using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaviLi.Domain.Entities;
using TaviLi.Application.Common.Interfaces;

namespace TaviLi.Infrastructure.Persistence
{
    // אנו יורשים מ-IdentityDbContext במקום מ-DbContext רגיל
    // מכיוון שזה אוטומטית מגדיר לנו את טבלאות ה-Identity (Users, Roles וכו')
    // אנו מציינים לו להשתמש במחלקות 'User' ו-'Role' המותאמות אישית שיצרנו
    public class ApplicationDbContext : IdentityDbContext<User, Role, string> , IApplicationDbContext
    {
        // מגדיר את הטבלה 'Missions' בבסיס הנתונים
        public DbSet<Mission> Missions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // כאן נגדיר את קשרי הגומלין (Relationships)
            // כפי שהגדרנו ב-Domain
            
            builder.Entity<Mission>(entity =>
            {
                // הגדרת הקשר "יוצר משימה" (Creator)
                // משימה אחת שייכת למשתמש אחד (יוצר)
                // ומשתמש אחד יכול ליצור מספר משימות
                entity.HasOne(m => m.CreatorUser)
                    .WithMany(u => u.CreatedMissions)
                    .HasForeignKey(m => m.CreatorUserId)
                    .OnDelete(DeleteBehavior.Restrict); // מניעת מחיקת משתמש אם יש לו משימות

                // הגדרת הקשר "שליח מבצע" (Courier)
                // משימה אחת שייכת למשתמש אחד (שליח) - או null
                // ומשתמש אחד יכול לבצע מספר משימות
                entity.HasOne(m => m.CourierUser)
                    .WithMany(u => u.AssignedMissions)
                    .HasForeignKey(m => m.CourierUserId)
                    .OnDelete(DeleteBehavior.SetNull); // אם שליח נמחק, המשימה חוזרת ל-null

                
            });
        }
    }
}