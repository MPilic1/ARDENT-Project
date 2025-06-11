using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Core.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Web.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
        public string Password { get; set; }

        [BindProperty]
        public string FirstName { get; set; }

        [BindProperty]
        public string LastName { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("QuizAppApi");
                var registerModel = new RegisterViewModel
                {
                    Username = Username,
                    Email = Email,
                    Password = Password,
                    FirstName = FirstName,
                    LastName = LastName
                };

                var response = await client.PostAsJsonAsync("api/auth/register", registerModel);
                
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseViewModel>();
                    
                    // Create claims for the user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, authResponse.Username),
                        new Claim(ClaimTypes.Email, authResponse.Email),
                        new Claim("FirstName", authResponse.FirstName ?? ""),
                        new Claim("LastName", authResponse.LastName ?? "")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToPage("/Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, error);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration");
                return Page();
            }
        }
    }
} 