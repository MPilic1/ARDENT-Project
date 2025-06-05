using Microsoft.AspNetCore.Mvc;
using QuizApp.Core.Models;
using QuizApp.Web.Services;

namespace QuizApp.Web.Controllers
{
    public class GameController : Controller
    {
        private readonly QuizApiService _quizApiService;

        public GameController(QuizApiService quizApiService)
        {
            _quizApiService = quizApiService;
        }

        public async Task<IActionResult> Host(int quizId)
        {
            try
            {
                var gameSession = await _quizApiService.StartGameAsync(quizId);
                return View(gameSession);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Play(string gameCode, int playerId)
        {
            try
            {
                var gameSession = await _quizApiService.GetGameSessionAsync(gameCode);
                if (gameSession == null)
                {
                    return RedirectToAction("JoinGame", "Home");
                }

                ViewBag.PlayerId = playerId;
                return View(gameSession);
            }
            catch
            {
                return RedirectToAction("JoinGame", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] PlayerAnswer answer)
        {
            try
            {
                var result = await _quizApiService.SubmitAnswerAsync(answer);
                return Json(new { success = true, score = result.ScoreEarned });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> Leaderboard(int gameSessionId)
        {
            var leaderboard = await _quizApiService.GetLeaderboardAsync(gameSessionId);
            return PartialView("_LeaderboardPartial", leaderboard);
        }
    }
} 