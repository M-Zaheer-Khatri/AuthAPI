using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Repository.Interface;
using Microsoft.AspNetCore.Identity;

namespace AuthAPI.Repository.Service
{
    public class UserAuthRepository : IUserAuth
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserAuthRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> CreateUserAsync(RegisterModel registerModel)
        {
            var user = new ApplicationUser
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
                Name = registerModel.Name ?? string.Empty,
            };
            var result = await _userManager.CreateAsync(user, registerModel.Password);
            return result;
        }

        public async Task<T?> FindByUserEmailAsync<T>(string email) where T : class
        {
            if(typeof(T) == typeof(ApplicationUser))
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                return existingUser as T;
            }
            return null;
        }

        public async Task<SignInResult?> PasswordSignInUserAsync<T>(T model) where T : class
        {
            // Dynamically extract properties using reflection
            try
            {
                var email = model?.GetType().GetProperty("Email")?.GetValue(model, null)?.ToString() ?? string.Empty;
                var password = model?.GetType().GetProperty("Password")?.GetValue(model, null)?.ToString() ?? string.Empty;
                var isPersistent = model?.GetType().GetProperty("RememberMe")?.GetValue(model, null) as bool? ?? false;

                var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent, true);
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if necessary
                return SignInResult.Failed;
            }
        }
    }
}
