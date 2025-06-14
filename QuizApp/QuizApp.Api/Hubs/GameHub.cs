using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace QuizApp.Api.Hubs
{
    public class GameHub : Hub
    {
        private readonly QuizAppContext _context;
        private readonly ILogger<GameHub> _logger;

        public GameHub(QuizAppContext context, ILogger<GameHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task JoinGame(string gameCode, string playerId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            
            // Get player details and notify others
            if (!string.IsNullOrEmpty(playerId) && int.TryParse(playerId, out int playerIdInt))
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.Id == playerIdInt);
                
                if (player != null)
                {
                    await Clients.OthersInGroup(gameCode).SendAsync("PlayerJoined", player);
                }
            }
        }

        public async Task LeaveGame(string gameCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameCode);
        }

        public async Task HostGame(string gameCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            
            // Send existing players to the host
            var gameSession = await _context.GameSessions
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Code == gameCode);

            if (gameSession != null)
            {
                foreach (var player in gameSession.Players)
                {
                    await Clients.Caller.SendAsync("PlayerJoined", player);
                }
            }
        }

        public async Task StartGame(string gameCode)
        {
            try
            {
                // Find the game session
                var gameSession = await _context.GameSessions
                    .Include(g => g.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(g => g.Code == gameCode);

                if (gameSession == null)
                {
                    await Clients.Caller.SendAsync("StartGameFailed", "Game session not found");
                    return;
                }

                // Check if game has questions
                if (gameSession.Quiz?.Questions == null || !gameSession.Quiz.Questions.Any())
                {
                    await Clients.Caller.SendAsync("StartGameFailed", "Quiz has no questions");
                    return;
                }

                // Update game status to InProgress
                gameSession.Status = GameStatus.InProgress;
                gameSession.StartTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify all players that the game has started
                await Clients.Group(gameCode).SendAsync("GameStarted");

                // Send the first question
                var firstQuestion = gameSession.Quiz.Questions.OrderBy(q => q.Id).FirstOrDefault();
                if (firstQuestion != null)
                {
                    var questionData = new
                    {
                        questionNumber = 1,
                        totalQuestions = gameSession.Quiz.Questions.Count,
                        text = firstQuestion.Text,
                        answers = firstQuestion.Answers.Select(a => a.Text).ToArray(),
                        correctAnswerIndex = firstQuestion.Answers.ToList().FindIndex(a => a.IsCorrect),
                        timeLimit = firstQuestion.TimeLimit,
                        questionId = firstQuestion.Id,
                        points = firstQuestion.Points
                    };
                    await Clients.Group(gameCode).SendAsync("QuestionReceived", questionData);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("StartGameFailed", ex.Message);
            }
        }

        public async Task<object> SubmitAnswer(string gameCode, int questionId, int answerIndex, int playerId)
        {
            try
            {
                // Find the question and validate answer
                var question = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == questionId);

                if (question == null) return null;

                // Find the player
                var player = await _context.Players.FindAsync(playerId);
                if (player == null) return null;

                // Calculate score (simple scoring: full points if correct, 0 if wrong)
                var answers = question.Answers.ToList();
                var selectedAnswer = answerIndex >= 0 && answerIndex < answers.Count ? answers[answerIndex] : null;
                
                int scoreEarned = 0;
                bool isCorrect = false;

                if (selectedAnswer != null && selectedAnswer.IsCorrect)
                {
                    scoreEarned = question.Points;
                    isCorrect = true;
                }

                // Find the game session to set GameSessionId
                var gameSession = await _context.GameSessions
                    .Include(g => g.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(gs => gs.Code == gameCode);

                if (gameSession == null) return null;

                // Save the player's answer
                var playerAnswer = new PlayerAnswer
                {
                    GameSessionId = gameSession.Id,
                    PlayerId = playerId,
                    QuestionId = questionId,
                    AnswerId = selectedAnswer?.Id ?? 0,
                    ScoreEarned = scoreEarned,
                    AnsweredAt = DateTime.UtcNow
                };

                _context.PlayerAnswers.Add(playerAnswer);
                await _context.SaveChangesAsync();

                var result = new { 
                    isCorrect = isCorrect, 
                    score = scoreEarned, 
                    correctAnswerIndex = question.Answers.ToList().FindIndex(a => a.IsCorrect),
                    answerIndex,
                    maxPoints = question.Points
                };

                // Send confirmation to the player
                await Clients.Caller.SendAsync("AnswerSubmitted", result);

                // Get all questions for this quiz
                var questions = gameSession.Quiz.Questions.OrderBy(q => q.Id).ToList();
                var currentQuestionIndex = questions.FindIndex(q => q.Id == questionId);

                // If this was a timer expiration or we're at the last question
                if (answerIndex == -1 || currentQuestionIndex >= questions.Count - 1)
                {
                    if (currentQuestionIndex >= questions.Count - 1)
                    {
                        // Game finished, show final results
                        await EndGame(gameCode);
                    }
                    else
                    {
                        // Move to next question
                        var nextQuestion = questions[currentQuestionIndex + 1];
                        var questionData = new
                        {
                            questionNumber = currentQuestionIndex + 2,
                            totalQuestions = questions.Count,
                            text = nextQuestion.Text,
                            answers = nextQuestion.Answers.Select(a => a.Text).ToArray(),
                            correctAnswerIndex = nextQuestion.Answers.ToList().FindIndex(a => a.IsCorrect),
                            timeLimit = nextQuestion.TimeLimit,
                            questionId = nextQuestion.Id,
                            points = nextQuestion.Points
                        };
                        
                        // Send next question to all players
                        await Clients.Group(gameCode).SendAsync("QuestionReceived", questionData);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("AnswerSubmitFailed", ex.Message);
                return null;
            }
        }

        public async Task NextQuestion(string gameCode, int currentQuestionNumber)
        {
            try
            {
                var gameSession = await _context.GameSessions
                    .Include(g => g.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(g => g.Code == gameCode);

                if (gameSession == null)
                {
                    await Clients.Caller.SendAsync("GameError", "Game session not found");
                    return;
                }

                // Get total number of questions
                var totalQuestions = await _context.Questions
                    .CountAsync(q => q.QuizId == gameSession.QuizId);

                // Get the next question
                var nextQuestion = await _context.Questions
                    .Include(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.QuizId == gameSession.QuizId && q.Id > currentQuestionNumber);

                if (nextQuestion == null)
                {
                    // No more questions, end the game
                    await EndGame(gameCode);
                    return;
                }

                // Send the next question to all players
                var questionData = new
                {
                    questionId = nextQuestion.Id,
                    questionNumber = currentQuestionNumber + 1,
                    totalQuestions = totalQuestions,
                    text = nextQuestion.Text,
                    timeLimit = nextQuestion.TimeLimit,
                    answers = nextQuestion.Answers.Select(a => a.Text).ToArray(),
                    correctAnswerIndex = nextQuestion.Answers.ToList().FindIndex(a => a.IsCorrect),
                    points = nextQuestion.Points
                };

                await Clients.Group(gameCode).SendAsync("QuestionReceived", questionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NextQuestion");
                await Clients.Caller.SendAsync("GameError", "Failed to get next question");
            }
        }

        public async Task EndGame(string gameCode)
        {
            try
            {
                var gameSession = await _context.GameSessions
                    .Include(g => g.Players)
                    .Include(g => g.PlayerAnswers)
                    .FirstOrDefaultAsync(g => g.Code == gameCode);

                if (gameSession == null) return;

                // Update game status
                gameSession.Status = GameStatus.Completed;
                gameSession.EndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Calculate final scores
                var finalScores = gameSession.Players.Select(p => new
                {
                    playerId = p.Id,
                    playerName = p.Name,
                    totalScore = gameSession.PlayerAnswers
                        .Where(pa => pa.PlayerId == p.Id)
                        .Sum(pa => pa.ScoreEarned),
                    correctAnswers = gameSession.PlayerAnswers
                        .Where(pa => pa.PlayerId == p.Id && pa.ScoreEarned > 0)
                        .Count()
                }).OrderByDescending(p => p.totalScore).ToList();

                // Update the final scores in the database
                foreach (var score in finalScores)
                {
                    var player = await _context.Players.FindAsync(score.playerId);
                    if (player != null)
                    {
                        player.Score = score.totalScore;
                        _context.Entry(player).State = EntityState.Modified;
                    }
                }
                await _context.SaveChangesAsync();

                await Clients.Group(gameCode).SendAsync("GameEnded", finalScores);
            }
            catch (Exception ex)
            {
                await Clients.Group(gameCode).SendAsync("GameError", ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Handle player disconnection
            await base.OnDisconnectedAsync(exception);
        }
    }
} 