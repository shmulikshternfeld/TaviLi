using Microsoft.AspNetCore.Identity;

namespace TaviLi.Domain.Entities
{
    
    // מחלקה זו יורשת את כל שדות Identity (אימייל, סיסמה וכו') 
    // ומוסיפה את השדות הייעודיים שלנו.
    public class User : IdentityUser
    {
        // שם מלא של המשתמש
        public required string Name { get; set; }

        // נתיב לתמונת פרופיל
        public string? ProfileImageUrl { get; set; }

        // --- קשרי גומלין ---
        // משתמש יכול ליצור מספר משימות
        public virtual ICollection<Mission> CreatedMissions { get; set; } = new List<Mission>();

        // משתמש יכול להיות שליח במספר משימות
        public virtual ICollection<Mission> AssignedMissions { get; set; } = new List<Mission>();
    }
}