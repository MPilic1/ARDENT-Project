using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Api.Controllers;
using QuizApp.Core.ViewModels;
using QuizApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuizApp.Tests.Quiz
{
    public class CreateQuizValidTest
    {
        [Fact]
        public async Task CreateQuiz_ValidQuiz_ReturnsCreatedQuiz()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "QuizTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            var quizViewModel = new QuizViewModel
            {
                Title = "Sample Quiz",
                Description = "A test quiz",
                TimeLimit = "00:30:00",
                IsActive = true,
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Text = "What is 2+2?",
                        Points = 100,
                        TimeLimit = 20,
                        Answers = new List<AnswerViewModel>
                        {
                            new AnswerViewModel { Text = "4", IsCorrect = true },
                            new AnswerViewModel { Text = "3", IsCorrect = false }
                        }
                    }
                }
            };

            var result = await controller.CreateQuiz(quizViewModel);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdQuiz = Assert.IsType<QuizApp.Core.Models.Quiz>(createdResult.Value);
            Assert.Equal("Sample Quiz", createdQuiz.Title);
            Assert.Single(createdQuiz.Questions);
            Assert.Equal(2, createdQuiz.Questions.First().Answers.Count);
        }
    }
} 