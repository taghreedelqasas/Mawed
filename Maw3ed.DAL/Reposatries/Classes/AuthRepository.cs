using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Maw3ed.DAL
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthRepository(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // ---- Register ----
        public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
            => _userManager.CreateAsync(user, password);

        // ---- Roles ----
        public Task<bool> RoleExistsAsync(string roleName)
            => _roleManager.RoleExistsAsync(roleName);

        public Task<IdentityResult> CreateRoleAsync(string roleName)
            => _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName)
            => _userManager.AddToRoleAsync(user, roleName);

        public Task<IList<string>> GetRolesAsync(ApplicationUser user)
            => _userManager.GetRolesAsync(user);

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName)
            => _userManager.IsInRoleAsync(user, roleName);

        // ---- Lookup ----
        public Task<ApplicationUser?> FindByEmailAsync(string email)
            => _userManager.FindByEmailAsync(email);

        public Task<ApplicationUser?> FindByIdAsync(string id)
            => _userManager.FindByIdAsync(id);

        public Task<ApplicationUser?> FindByUserNameAsync(string userName)
            => _userManager.FindByNameAsync(userName);

        // ---- Login ----
        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
            => _userManager.CheckPasswordAsync(user, password);

        public Task<bool> IsLockedOutAsync(ApplicationUser user)
            => _userManager.IsLockedOutAsync(user);

        public Task AccessFailedAsync(ApplicationUser user)
            => _userManager.AccessFailedAsync(user);

        public Task ResetAccessFailedCountAsync(ApplicationUser user)
            => _userManager.ResetAccessFailedCountAsync(user);

        //foremail 
        public Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password)
           => _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        // ---- Update / misc ----
        public Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
            => _userManager.UpdateAsync(user);

        public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
            => _userManager.GenerateEmailConfirmationTokenAsync(user);

        public Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
            => _userManager.ConfirmEmailAsync(user, token);

        public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
            => _userManager.GeneratePasswordResetTokenAsync(user);

        public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
            => _userManager.ResetPasswordAsync(user, token, newPassword);

        //Others 
        public async Task<bool> AnyAsync(Expression<Func<ApplicationUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users.AnyAsync(predicate, cancellationToken);
        }
    }
}
