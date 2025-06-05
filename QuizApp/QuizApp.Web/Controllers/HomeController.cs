using Microsoft.AspNetCore.Mvc;
using QuizApp.Web.Services;
using Microsoft.Extensions.Logging;

namespace QuizApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuizApiService _quizApiService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(QuizApiService quizApiService, ILogger<HomeController> logger)
        {
            _quizApiService = quizApiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching quizzes for home page");
                var quizzes = await _quizApiService.GetQuizzesAsync();
                return View(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quizzes for home page");
                // Don't expose the error to the user, just pass an empty list
                return View(Array.Empty<QuizApp.Core.Models.Quiz>());
            }
        }

        public IActionResult JoinGame()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> JoinGame(string gameCode, string playerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameCode) || string.IsNullOrWhiteSpace(playerName))
                {
                    ModelState.AddModelError("", "Game code and player name are required.");
                    return View();
                }

                _logger.LogInformation("Attempting to join game {GameCode} for player {PlayerName}", gameCode, playerName);
                var player = await _quizApiService.JoinGameAsync(gameCode, playerName);
                return RedirectToAction("Play", "Game", new { gameCode = gameCode, playerId = player.Id });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error joining game {GameCode} for player {PlayerName}", gameCode, playerName);
                ModelState.AddModelError("", "Unable to connect to the game server. Please try again.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining game {GameCode} for player {PlayerName}", gameCode, playerName);
                ModelState.AddModelError("", "Unable to join game. Please check the game code and try again.");
                return View();
            }
        }

        [Route("/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
} 