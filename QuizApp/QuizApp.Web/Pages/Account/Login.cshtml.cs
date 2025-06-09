using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Core.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace QuizApp.Web.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLoginAsync(string Username, string Password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("QuizAppApi");
                var loginModel = new LoginViewModel
                {
                    Username = Username,
                    Password = Password
                };

                var response = await client.PostAsJsonAsync("api/auth/login", loginModel);
                
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
                        IsPersistent = false, // Session cookie (non-persistent)
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Expires after 1 hour of inactivity
                    };

                    // Sign in the user
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToPage("/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRegisterAsync(string Username, string Email, string Password, string FirstName, string LastName)
        {
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
                        IsPersistent = false, // Session cookie (non-persistent)
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Expires after 1 hour of inactivity
                    };

                    // Sign in the user
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