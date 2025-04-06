using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserAuth _userAuthRepo;
        private readonly string? _JwtKey;
        private readonly string? _JwtIssuer;
        private readonly string? _JwtAudience;
        private readonly int _JwtExpiry;

        public UserAuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            IEmailSender emailSender, IUserAuth userAuthRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _userAuthRepo = userAuthRepo;
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
            //var existingUser = await _userManager.FindByEmailAsync(registerModel.Email);
            var existingUser = await _userAuthRepo.FindByUserEmailAsync<ApplicationUser>(registerModel.Email);
            if (existingUser != null)
            {
                return Conflict("Email already Exist");
            }

            //var result = await _userManager.CreateAsync(user, registerModel.Password);
            var result = await _userAuthRepo.CreateUserAsync(registerModel);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            else
            {
                
                //await _emailSender.SendEmailAsync(registerModel, "Registration Successful", "You have successfully registered to our system");
                await _emailSender.SendEmailAsync<RegisterModel>(registerModel, "Registration Successful", "You have successfully registered to our system");
            }
                return Ok("User Created Successfully!");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userAuthRepo.FindByUserEmailAsync<ApplicationUser>(loginModel.Email);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }

            var result = await _userAuthRepo.PasswordSignInUserAsync<LoginModel>(loginModel);
            if (result == null)
            {
                return Unauthorized(new { success = false, message = "You are not registered" });
            }
            if (result.IsNotAllowed)
            {
                return Unauthorized(new { success = false, message = "User is not allowed to login" });
            }
            else if (result.IsLockedOut)
            {
                return Unauthorized(new { success = false, message = "User is locked out" });
            }
            else if (!result.Succeeded)
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }
            else
            {
                var token = GeneratedJwtToken(user);
                return Ok(new { success = true, token, userName = user.Name, message = "Login Successfull!" });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("User Logged out Successfully");
        }
        private string GeneratedJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub ,user.Id),
                new Claim(JwtRegisteredClaimNames.Email , user.Email),
                new Claim("Name" , user.Name),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                claims: claims,
                expires: DateTime.Now.AddMinutes(_JwtExpiry),
                signingCredentials: creds
            );
            Console.WriteLine($"JWT Key Bytes Length: {Encoding.UTF8.GetBytes(_JwtKey).Length}");
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
