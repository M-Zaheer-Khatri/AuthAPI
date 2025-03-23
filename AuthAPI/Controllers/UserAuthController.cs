using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string? _JwtKey;
        private readonly string? _JwtIssuer;
        private readonly string? _JwtAudience;
        private readonly int _JwtExpiry;

        public UserAuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _JwtKey = configuration["Jwt:Key"];
            _JwtIssuer = configuration["Jwt:Issuer"];
            _JwtAudience = configuration["Jwt:Audience"];
            _JwtExpiry = int.Parse(configuration["Jwt:ExpiryMinutes"]);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (registerModel == null || string.IsNullOrEmpty(registerModel.Name) || string.IsNullOrEmpty(registerModel.Email) || string.IsNullOrEmpty(registerModel.Password))
            {
                return BadRequest("Invalid Registration details");
            }
            var existingUser = await _userManager.FindByEmailAsync(registerModel.Email);
            if (existingUser != null)
            {
                return Conflict("Email already Exist");
            }

            var user = new ApplicationUser
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
                Name = registerModel.Name,
            };

            var result = await _userManager.CreateAsync(user, registerModel.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok("User Created Successfully!");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null)
            {
                return Unauthorized(new {success = false, message = "Invalid username or password"});
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginModel.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }

            var token = GenerateJwtToken(user);
            return Ok(new {success = true, token});
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            return "asdf";
        }
    }
}
