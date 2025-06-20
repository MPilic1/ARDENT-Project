using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace QuizApp.Tests.EditQuiz
{
    public class UpdateQuizValidTest
    {
        [Fact]
        public async Task UpdateQuiz_ValidData_QuizUpdated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "UpdateQuizTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            // Add an existing quiz
            var quiz = new QuizApp.Core.Models.Quiz
            {
                Title = "Old Title",
                Description = "Old Description",
                TimeLimit = TimeSpan.FromMinutes(30),
                IsActive = true,
                Questions = new List<Question>
                {
                    new Question
                    {
                        Text = "Old Question?",
                        Points = 100,
                        TimeLimit = 20,
                        Answers = new List<Answer>
                        {
                            new Answer { Text = "Old Answer", IsCorrect = true }
                        }
                    }
                }
            };
            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            // Fix: Set up a fake HttpContext so controller.ModelState is valid
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Prepare new data
            var updatedViewModel = new QuizViewModel
            {
                Title = "New Title",
                Description = "New Description",
                TimeLimit = "00:45:00",
                IsActive = false,
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Text = "New Question?",
                        Points = 200,
                        TimeLimit = 30,
                        Answers = new List<AnswerViewModel>
                        {
                            new AnswerViewModel { Text = "New Answer", IsCorrect = true }
                        }
                    }
                }
            };

            // Act
            var result = await controller.UpdateQuiz(quiz.Id, updatedViewModel);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify the quiz was updated in the database
            var updatedQuiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == quiz.Id);

            Assert.Equal("New Title", updatedQuiz.Title);
            Assert.Equal("New Description", updatedQuiz.Description);
            Assert.Equal(TimeSpan.FromMinutes(45), updatedQuiz.TimeLimit);
            Assert.False(updatedQuiz.IsActive);
            Assert.Single(updatedQuiz.Questions);
            Assert.Equal("New Question?", updatedQuiz.Questions.First().Text);
            Assert.Single(updatedQuiz.Questions.First().Answers);
            Assert.Equal("New Answer", updatedQuiz.Questions.First().Answers.First().Text);
        }
    }
} 