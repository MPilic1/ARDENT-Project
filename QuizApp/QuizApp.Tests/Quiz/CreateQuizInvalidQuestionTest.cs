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
    public class CreateQuizInvalidQuestionTest
    {
        [Fact]
        public async Task CreateQuiz_InvalidQuestionData_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "QuizTestDb3")
                .Options;
            using var context = new QuizAppContext(options);

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var quizWithInvalidQuestion = new QuizViewModel
            {
                Title = "Valid Quiz",
                Description = "A test quiz",
                TimeLimit = "00:30:00",
                IsActive = true,
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Text = "", // Empty question text
                        Points = -100, // Negative points
                        TimeLimit = -1, // Negative time limit
                        Answers = new List<AnswerViewModel>() // No answers
                    }
                }
            };

            // Add model validation errors
            controller.ModelState.AddModelError("Questions[0].Text", "Question text is required");
            controller.ModelState.AddModelError("Questions[0].Points", "Points must be positive");
            controller.ModelState.AddModelError("Questions[0].TimeLimit", "Time limit must be positive");
            controller.ModelState.AddModelError("Questions[0].Answers", "At least one answer is required");

            // Act
            var result = await controller.CreateQuiz(quizWithInvalidQuestion);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult.Value);
        }
    }
} 