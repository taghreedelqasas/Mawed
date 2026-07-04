using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Maw3ed.DAL
{
    public interface IAuthRepository
    {
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);

        // ---- Roles ----
        Task<bool> RoleExistsAsync(string roleName);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<bool> IsInRoleAsync(ApplicationUser user, string roleName);

        // ---- Lookup ----
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByIdAsync(string id);
        Task<ApplicationUser?> FindByUserNameAsync(string userName);

        // ---- Login (password check, no cookie sign-in since this is JWT) ----
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<bool> IsLockedOutAsync(ApplicationUser user);
        Task AccessFailedAsync(ApplicationUser user);
        Task ResetAccessFailedCountAsync(ApplicationUser user);

        //new for mail 
        Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password);

        // ---- Update / misc ----
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);

        //Others
        Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
