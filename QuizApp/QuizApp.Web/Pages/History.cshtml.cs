using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using QuizApp.Core.ViewModels;
using System.Net.Http.Json;

namespace QuizApp.Web.Pages
{
    public class HistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HistoryModel> _logger;

        public List<QuizHistoryViewModel> QuizHistory { get; set; } = new List<QuizHistoryViewModel>();
        public bool IsAuthenticated { get; set; }

        public HistoryModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<HistoryModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                IsAuthenticated = false;
                return;
            }

            IsAuthenticated = true;

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("User ID not found in claims for user: {Username}", User.Identity.Name);
                    return;
                }

                var client = _httpClientFactory.CreateClient("QuizAppApi");
                var response = await client.GetAsync($"api/game/history/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    QuizHistory = await response.Content.ReadFromJsonAsync<List<QuizHistoryViewModel>>() ?? new List<QuizHistoryViewModel>();
                }
                else
                {
                    _logger.LogError("Failed to fetch quiz history. Status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz history for user: {Username}", User.Identity.Name);
            }
        }
    }
} 