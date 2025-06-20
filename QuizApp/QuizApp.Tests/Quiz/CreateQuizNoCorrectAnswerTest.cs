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
using Microsoft.AspNetCore.Http;

namespace QuizApp.Tests.Quiz
{
    public class CreateQuizNoCorrectAnswerTest
    {
        [Fact]
        public async Task CreateQuiz_NoCorrectAnswer_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "QuizTestDb4")
                .Options;
            using var context = new QuizAppContext(options);

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var quizWithNoCorrectAnswer = new QuizViewModel
            {
                Title = "Valid Quiz",
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
                            new AnswerViewModel { Text = "3", IsCorrect = false },
                            new AnswerViewModel { Text = "5", IsCorrect = false }
                        }
                    }
                }
            };

            // Add model validation error
            controller.ModelState.AddModelError("Questions[0].Answers", "At least one answer must be correct");

            // Act
            var result = await controller.CreateQuiz(quizWithNoCorrectAnswer);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult.Value);
        }
    }
} 