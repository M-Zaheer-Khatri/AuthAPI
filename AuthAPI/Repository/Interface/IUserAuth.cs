using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthAPI.Repository.Interface
{
    public interface IUserAuth
    {
        public Task<IdentityResult> CreateUserAsync(RegisterModel user);

        public Task<T?> FindByUserEmailAsync<T>(string user) where T: class;

        public Task<SignInResult?> PasswordSignInUserAsync<T>(T model) where T : class;
    }
}
