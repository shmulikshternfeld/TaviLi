using TaviLi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaviLi.Application.Common.Interfaces
{
    public interface IAuthService
    {
        // חוזה לפונקציה שתיצור טוקן
        Task<string> CreateTokenAsync(User user, IList<string> roles);
    }
}