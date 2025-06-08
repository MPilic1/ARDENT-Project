using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using QuizApp.Core.Security;
using QuizApp.Infrastructure.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly QuizAppContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(QuizAppContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseViewModel>> Register(RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Received registration request for user: {Username}", model.Username);

                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    _logger.LogWarning("Username {Username} already exists", model.Username);
                    return BadRequest("Username already exists");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    _logger.LogWarning("Email {Email} already exists", model.Email);
                    return BadRequest("Email already exists");
                }

                // Generate password hash and salt
                string salt = PasswordHashProvider.GetSalt();
                string hash = PasswordHashProvider.GetHash(model.Password, salt);

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} registered successfully", model.Username);

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return new AuthResponseViewModel
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Username}", model.Username);
                return StatusCode(500, "An error occurred while registering the user");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseViewModel>> Login(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation("Received login request for user: {Username}", model.Username);

                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Username} not found", model.Username);
                    return Unauthorized("Invalid username or password");
                }

                // Verify password
                if (!PasswordHashProvider.VerifyPassword(model.Password, user.PasswordSalt, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", model.Username);
                    return Unauthorized("Invalid username or password");
                }

                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} logged in successfully", model.Username);

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return new AuthResponseViewModel
                {
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {Username}", model.Username);
                return StatusCode(500, "An error occurred while logging in");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 