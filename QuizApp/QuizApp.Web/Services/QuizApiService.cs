using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;

namespace QuizApp.Web.Services
{
    public class QuizApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuizApiService> _logger;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public QuizApiService(IConfiguration configuration, HttpClient httpClient, ILogger<QuizApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"]?.TrimEnd('/');
            
            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new InvalidOperationException("API Base URL is not configured. Please check ApiSettings:BaseUrl in appsettings.json");
            }
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _logger.LogInformation("QuizApiService initialized with base URL: {BaseUrl}", _baseUrl);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Quiz>> GetQuizzesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching quizzes from API");
                var response = await _httpClient.GetAsync("api/quiz");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch quizzes. Status code: {StatusCode}", response.StatusCode);
                    return Array.Empty<Quiz>();
                }

                var quizzes = await response.Content.ReadFromJsonAsync<IEnumerable<Quiz>>();
                return quizzes ?? Array.Empty<Quiz>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quizzes from API");
                return Array.Empty<Quiz>();
            }
        }

        public async Task<Quiz> GetQuizAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/quiz/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Quiz>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz {QuizId}", id);
                throw;
            }
        }

        public async Task<Quiz> CreateQuizAsync(QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Creating quiz with data: {QuizData}", 
                    JsonSerializer.Serialize(viewModel, _jsonOptions));

                var response = await _httpClient.PostAsJsonAsync("api/quiz", viewModel);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Quiz>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz");
                throw;
            }
        }

        public async Task<Quiz> UpdateQuizAsync(int id, QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Updating quiz {QuizId} with data: {QuizData}", 
                    id, JsonSerializer.Serialize(viewModel, _jsonOptions));

                var response = await _httpClient.PutAsJsonAsync($"api/quiz/{id}", viewModel);
                response.EnsureSuccessStatusCode();
                
                // Return the updated quiz
                return await GetQuizAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}", id);
                throw;
            }
        }

        public async Task<GameSession> StartGameAsync(int quizId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/game/start/{quizId}", null);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<GameSession>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting game for quiz {QuizId}", quizId);
                throw;
            }
        }

        public async Task<Player> JoinGameAsync(string gameCode, string playerName)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/game/join?gameCode={gameCode}&playerName={playerName}", new {});
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Player>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining game {GameCode} for player {PlayerName}", gameCode, playerName);
                throw;
            }
        }

        public async Task<Question> GetNextQuestionAsync(int gameSessionId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/game/{gameSessionId}/next-question", null);
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return null;
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Question>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next question for game session {GameSessionId}", gameSessionId);
                throw;
            }
        }

        public async Task<PlayerAnswer> SubmitAnswerAsync(PlayerAnswer answer)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/game/answer", answer);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PlayerAnswer>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer");
                throw;
            }
        }

        public async Task<IEnumerable<Player>> GetLeaderboardAsync(int gameSessionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/game/{gameSessionId}/leaderboard");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch leaderboard. Status code: {StatusCode}", response.StatusCode);
                    return Array.Empty<Player>();
                }

                var players = await response.Content.ReadFromJsonAsync<IEnumerable<Player>>();
                return players ?? Array.Empty<Player>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard for game session {GameSessionId}", gameSessionId);
                return Array.Empty<Player>();
            }
        }

        public async Task<GameSession> GetGameSessionAsync(string gameCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/game/session/{gameCode}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch game session {GameCode}. Status code: {StatusCode}", gameCode, response.StatusCode);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<GameSession>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching game session {GameCode}", gameCode);
                return null;
            }
        }
    }
} 