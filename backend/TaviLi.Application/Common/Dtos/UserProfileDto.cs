namespace TaviLi.Application.Common.Dtos
{
    // DTO המייצג משתמש להחזרה ללקוח
    public class UserProfileDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string Name { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public required IList<string> Roles { get; set; }
    }
}