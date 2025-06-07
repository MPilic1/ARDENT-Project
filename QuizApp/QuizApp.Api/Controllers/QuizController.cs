using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using QuizApp.Core.Extensions;
using QuizApp.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly QuizAppContext _context;
        private readonly ILogger<QuizController> _logger;

        public QuizController(QuizAppContext context, ILogger<QuizController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetQuizzes()
        {
            return await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return quiz;
        }

        [HttpPost]
        public async Task<ActionResult<Quiz>> CreateQuiz(QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Received quiz creation request: {@QuizViewModel}", viewModel);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (viewModel == null)
                {
                    _logger.LogWarning("Quiz view model is null");
                    return BadRequest("Quiz data is required.");
                }

                if (viewModel.Questions == null || viewModel.Questions.Count == 0)
                {
                    _logger.LogWarning("No questions provided in quiz");
                    return BadRequest("At least one question is required.");
                }

                // Convert TimeLimit string to TimeSpan
                if (!TimeSpan.TryParse(viewModel.TimeLimit, out TimeSpan timeLimit))
                {
                    _logger.LogWarning("Invalid time limit format: {TimeLimit}", viewModel.TimeLimit);
                    return BadRequest("Invalid time limit format. Use HH:mm:ss format.");
                }

                // Create Quiz entity using the mapping extension
                var quiz = viewModel.ToModel();
                quiz.CreatedAt = DateTime.UtcNow;
                quiz.CreatedBy = "Anonymous"; // We can update this when we add authentication

                // Validate questions and answers
                foreach (var question in quiz.Questions)
                {
                    if (question.Answers == null || question.Answers.Count == 0)
                    {
                        _logger.LogWarning("Question has no answers: {QuestionText}", question.Text);
                        return BadRequest($"Question '{question.Text}' must have at least one answer.");
                    }

                    if (!question.Answers.Any(a => a.IsCorrect))
                    {
                        _logger.LogWarning("Question has no correct answer: {QuestionText}", question.Text);
                        return BadRequest($"Question '{question.Text}' must have at least one correct answer.");
                    }
                }

                _logger.LogInformation("Adding quiz to database: {@Quiz}", quiz);
                _context.Quizzes.Add(quiz);
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Quiz created successfully with ID: {QuizId}", quiz.Id);

                // Reload the quiz with all navigation properties
                var createdQuiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == quiz.Id);

                return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, createdQuiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { error = "An error occurred while creating the quiz. Please try again." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, QuizViewModel viewModel)
        {
            try
            {
                _logger.LogInformation("Received quiz update request for ID {QuizId}", id);
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (viewModel == null)
                {
                    _logger.LogWarning("Quiz view model is null");
                    return BadRequest("Quiz data is required.");
                }

                if (viewModel.Questions == null || viewModel.Questions.Count == 0)
                {
                    _logger.LogWarning("No questions provided in quiz update");
                    return BadRequest("At least one question is required.");
                }

                // Convert TimeLimit string to TimeSpan
                
                if (!TimeSpan.TryParse(viewModel.TimeLimit, out TimeSpan timeLimit))
                {
                    _logger.LogWarning("Invalid time limit format: {TimeLimit}", viewModel.TimeLimit);
                    return BadRequest("Invalid time limit format. Use HH:mm:ss format.");
                }
                
                // Get the existing quiz with all related data
                
                var existingQuiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (existingQuiz == null)
                {
                    _logger.LogWarning("Quiz with ID {QuizId} not found", id);
                    return NotFound();
                }

                // Update basic quiz properties
                existingQuiz.Title = viewModel.Title;
                existingQuiz.Description = viewModel.Description;
                existingQuiz.TimeLimit = timeLimit;
                existingQuiz.IsActive = viewModel.IsActive;

                // Remove existing questions and their answers
                if (existingQuiz.Questions.Any())
                {
                    foreach (var existingQuestion in existingQuiz.Questions.ToList())
                    {
                        if (existingQuestion.Answers.Any())
                        {
                            _context.Answers.RemoveRange(existingQuestion.Answers);
                        }
                        _context.Questions.Remove(existingQuestion);
                    }
                    existingQuiz.Questions.Clear();
                }

                // Save changes to remove old data first
                await _context.SaveChangesAsync();

                // Add new questions and answers from viewModel
                foreach (var questionViewModel in viewModel.Questions)
                {
                    var question = new Question
                    {
                        Text = questionViewModel.Text,
                        Points = questionViewModel.Points,
                        TimeLimit = questionViewModel.TimeLimit,
                        Type = QuestionType.MultipleChoice,
                        QuizId = existingQuiz.Id
                    };

                    // Add answers to the question
                    if (questionViewModel.Answers != null)
                    {
                        foreach (var answerViewModel in questionViewModel.Answers)
                        {
                            var answer = new Answer
                            {
                                Text = answerViewModel.Text,
                                IsCorrect = answerViewModel.IsCorrect,
                                Question = question
                            };
                            question.Answers.Add(answer);
                        }
                    }

                    existingQuiz.Questions.Add(question);
                }

                // Validate questions and answers (same as Create)
                foreach (var question in existingQuiz.Questions)
                {
                    if (question.Answers == null || question.Answers.Count == 0)
                    {
                        _logger.LogWarning("Question has no answers: {QuestionText}", question.Text);
                        return BadRequest($"Question '{question.Text}' must have at least one answer.");
                    }

                    if (!question.Answers.Any(a => a.IsCorrect))
                    {
                        _logger.LogWarning("Question has no correct answer: {QuestionText}", question.Text);
                        return BadRequest($"Question '{question.Text}' must have at least one correct answer.");
                    }
                }

                // Save the final changes
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Quiz {QuizId} updated successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, new { error = "An error occurred while updating the quiz. Please try again." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
} 