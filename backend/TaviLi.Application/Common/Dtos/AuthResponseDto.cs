namespace TaviLi.Application.Common.Dtos
{
    // DTO המוחזר לאחר התחברות או הרשמה מוצלחת
    public class AuthResponseDto
    {
        public required UserProfileDto User { get; set; }
        public required string Token { get; set; }
    }
}