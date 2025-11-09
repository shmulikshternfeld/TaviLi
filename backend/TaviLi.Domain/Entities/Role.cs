using Microsoft.AspNetCore.Identity;

namespace TaviLi.Domain.Entities
{
    // אנו משתמשים במערכת התפקידים המובנית של Identity.
    // המחלקה 'IdentityRole' כבר מכילה שדה 'Name' עבור "Client" ו-"Courier".
    public class Role : IdentityRole
    {
        // אין צורך בשדות נוספים עבור ה-MVP.
    }
}