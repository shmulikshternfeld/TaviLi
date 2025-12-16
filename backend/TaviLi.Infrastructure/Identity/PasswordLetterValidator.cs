using Microsoft.AspNetCore.Identity;

namespace TaviLi.Infrastructure.Identity;

public class PasswordLetterValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
    {
        if (string.IsNullOrEmpty(password) || !password.Any(char.IsLetter))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRequiresLetter",
                Description = "הסיסמה חייבת להכיל לפחות אות אחת"
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
