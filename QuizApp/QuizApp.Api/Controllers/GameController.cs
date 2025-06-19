using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;
using QuizApp.Api.Hubs;
using QuizApp.Core.ViewModels;

namespace QuizApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly QuizAppContext _context;
        private readonly IHubContext<GameHub> _hubContext;

        public GameController(QuizAppContext context, IHubContext<GameHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("start/{quizId}")]
        public async Task<ActionResult<GameSession>> StartGame(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound("Quiz not found");
            }

            var gameSession = new GameSession
            {
                QuizId = quizId,
                Status = GameStatus.WaitingToStart
            };

            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGameSession), new { id = gameSession.Id }, gameSession);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameSession>> GetGameSession(int id)
        {
            var gameSession = await _context.GameSessions
                .Include(g => g.Quiz)
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gameSession == null)
            {
                return NotFound();
            }

            return gameSession;
        }

        [HttpPost("join")]
        public async Task<ActionResult<Player>> JoinGame(string gameCode, string playerName, int? userId = null)
        {
            var gameSession = await _context.GameSessions
                .FirstOrDefaultAsync(g => g.Code == gameCode && g.Status == GameStatus.WaitingToStart);

            if (gameSession == null)
            {
                return NotFound("Game session not found or already started");
            }

            // Prefer userId from parameter, fallback to claims
            if (userId == null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
                {
                    userId = id;
                }
            }

            var player = new Player
            {
                GameSessionId = gameSession.Id,
                Name = playerName,
                UserId = userId // Will be null for guests
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // The player will join the SignalR group and notify others through the hub
            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }

        [HttpGet("player/{id}")]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            return player;
        }

        [HttpPost("answer")]
        public async Task<ActionResult<PlayerAnswer>> SubmitAnswer([FromBody] PlayerAnswer playerAnswer)
        {
            var gameSession = await _context.GameSessions
                .Include(g => g.Quiz)
                .FirstOrDefaultAsync(g => g.Id == playerAnswer.GameSessionId);

            if (gameSession == null || gameSession.Status != GameStatus.InProgress)
            {
                return BadRequest("Invalid game session or game not in progress");
            }

            var answer = await _context.Answers.FindAsync(playerAnswer.AnswerId);
            if (answer == null)
            {
                return BadRequest("Invalid answer");
            }

            // Calculate score based on response time and correctness
            playerAnswer.ScoreEarned = answer.IsCorrect ? 
                (int)(1000 * (1 - (playerAnswer.ResponseTime.TotalSeconds / 20))) : 0;

            _context.PlayerAnswers.Add(playerAnswer);
            
            // Update player's total score
            var player = await _context.Players.FindAsync(playerAnswer.PlayerId);
            if (player != null)
            {
                player.Score += playerAnswer.ScoreEarned;
                _context.Entry(player).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            // Notify all players about the answer submission
            await _hubContext.Clients.Group(gameSession.Code).SendAsync("AnswerSubmitted", new
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Score = player.Score,
                IsCorrect = answer.IsCorrect
            });

            return CreatedAtAction(nameof(GetPlayerAnswer), new { id = playerAnswer.Id }, playerAnswer);
        }

        [HttpGet("answer/{id}")]
        public async Task<ActionResult<PlayerAnswer>> GetPlayerAnswer(int id)
        {
            var playerAnswer = await _context.PlayerAnswers.FindAsync(id);

            if (playerAnswer == null)
            {
                return NotFound();
            }

            return playerAnswer;
        }

        [HttpPost("{id}/next-question")]
        public async Task<ActionResult<Question>> GetNextQuestion(int id)
        {
            var gameSession = await _context.GameSessions
                .Include(g => g.Quiz)
                .ThenInclude(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gameSession == null)
            {
                return NotFound("Game session not found");
            }

            var answeredQuestions = await _context.PlayerAnswers
                .Where(pa => pa.GameSessionId == id)
                .Select(pa => pa.QuestionId)
                .Distinct()
                .ToListAsync();

            var nextQuestion = gameSession.Quiz.Questions
                .Where(q => !answeredQuestions.Contains(q.Id))
                .OrderBy(q => q.Id)
                .FirstOrDefault();

            if (nextQuestion == null)
            {
                gameSession.Status = GameStatus.Completed;
                gameSession.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify all players that the game is complete
                await _hubContext.Clients.Group(gameSession.Code).SendAsync("GameCompleted", await GetLeaderboard(id));
                return NoContent();
            }

            if (gameSession.Status == GameStatus.WaitingToStart)
            {
                gameSession.Status = GameStatus.InProgress;
                await _context.SaveChangesAsync();
            }

            // Notify all players about the new question
            await _hubContext.Clients.Group(gameSession.Code).SendAsync("NewQuestion", nextQuestion);

            return nextQuestion;
        }

        [HttpGet("{id}/leaderboard")]
        public async Task<ActionResult<IEnumerable<Player>>> GetLeaderboard(int id)
        {
            return await _context.Players
                .Where(p => p.GameSessionId == id)
                .OrderByDescending(p => p.Score)
                .ToListAsync();
        }

        [HttpGet("session/{gameCode}")]
        public async Task<ActionResult<GameSession>> GetGameSessionByCode(string gameCode)
        {
            var gameSession = await _context.GameSessions
                .Include(g => g.Quiz)
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Code == gameCode);

            if (gameSession == null)
            {
                return NotFound("Game session not found");
            }

            return gameSession;
        }

        [HttpGet("history/{userId}")]
        public async Task<ActionResult<IEnumerable<QuizHistoryViewModel>>> GetUserQuizHistory(int userId)
        {
            var players = await _context.Players
                .Where(p => p.UserId == userId)
                .Include(p => p.GameSession)
                .ThenInclude(gs => gs.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(p => p.GameSession)
                .ThenInclude(gs => gs.PlayerAnswers)
                .ThenInclude(pa => pa.Answer)
                .OrderByDescending(p => p.GameSession.StartTime)
                .ToListAsync();

            var history = players.Select(p => new QuizHistoryViewModel
            {
                GameSessionId = p.GameSession.Id,
                GameCode = p.GameSession.Code,
                QuizTitle = p.GameSession.Quiz.Title,
                PlayedAt = p.GameSession.StartTime,
                FinalScore = p.Score,
                TotalQuestions = p.GameSession.Quiz.Questions.Count,
                CorrectAnswers = p.GameSession.PlayerAnswers
                    .Where(pa => pa.PlayerId == p.Id && pa.Answer.IsCorrect)
                    .Count(),
                Duration = p.GameSession.EndTime.HasValue 
                    ? p.GameSession.EndTime.Value - p.GameSession.StartTime 
                    : TimeSpan.Zero,
                Status = p.GameSession.Status.ToString()
            }).ToList();

            return history;
        }
    }
} 