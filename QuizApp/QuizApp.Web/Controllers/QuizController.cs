using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using QuizApp.Core.Extensions;
using QuizApp.Web.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizApp.Web.Controllers
{
    public class QuizController : Controller
    {
        private readonly QuizApiService _quizApiService;
        private readonly ILogger<QuizController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public QuizController(QuizApiService quizApiService, ILogger<QuizController> logger)
        {
            _quizApiService = quizApiService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        [Authorize]
        public IActionResult Create()
        {
            return View(new QuizViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Received quiz creation request: {QuizData}", 
                    JsonSerializer.Serialize(viewModel, _jsonOptions));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Invalid quiz model state: {Errors}", string.Join("; ", errors));
                    return BadRequest(new { errors = errors });
                }

                var createdQuiz = await _quizApiService.CreateQuizAsync(viewModel);
                
                _logger.LogInformation("Quiz created successfully with ID: {QuizId}", createdQuiz.Id);
                
                // Return the URL to redirect to
                return Ok(Url.Action("Details", new { id = createdQuiz.Id }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { errors = new[] { "Unable to create quiz. Please try again." } });
            }
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var quiz = await _quizApiService.GetQuizAsync(id);
                if (quiz == null)
                {
                    return NotFound();
                }
                return View(quiz.ToViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz {QuizId} for editing", id);
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromBody] QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Received quiz update request for ID {QuizId}: {QuizData}", 
                    id, JsonSerializer.Serialize(viewModel, _jsonOptions));

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    _logger.LogWarning("Invalid quiz model state: {Errors}", string.Join("; ", errors));
                    return BadRequest(new { errors = errors });
                }

                var updatedQuiz = await _quizApiService.UpdateQuizAsync(id, viewModel);
                
                _logger.LogInformation("Quiz updated successfully with ID: {QuizId}", updatedQuiz.Id);
                
                // Return the URL to redirect to
                return Ok(Url.Action("Details", new { id = updatedQuiz.Id }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { errors = new[] { "Unable to update quiz. Please try again." } });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddQuestion(int quizId, QuestionViewModel questionViewModel)
        {
            var quiz = await _quizApiService.GetQuizAsync(quizId);
            if (quiz == null)
            {
                return NotFound();
            }

            var question = new Question
            {
                Text = questionViewModel.Text,
                Points = questionViewModel.Points,
                TimeLimit = questionViewModel.TimeLimit,
                Type = QuestionType.MultipleChoice,
                Answers = questionViewModel.Answers?.Select(a => new Answer
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            };

            quiz.Questions.Add(question);
            await _quizApiService.CreateQuizAsync(quiz.ToViewModel());

            return RedirectToAction("Edit", new { id = quizId });
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var quiz = await _quizApiService.GetQuizAsync(id);
                if (quiz == null)
                {
                    return NotFound();
                }
                return View(quiz.ToViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz {QuizId} details", id);
                return RedirectToAction("Index", "Home");
            }
        }
    }
} 