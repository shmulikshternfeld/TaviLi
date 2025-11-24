namespace TaviLi.Application.Common.Interfaces
    {
        public interface ICurrentUserService
        {
            // יחזיר את ה-ID של המשתמש מהטוקן
            string? GetUserId();
            string? GetUserEmail();
            string? GetUserName();
        }
    }