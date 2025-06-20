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
    public class CreateQuizInvalidDataTest
    {
        [Fact]
        public async Task CreateQuiz_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "QuizTestDb2")
                .Options;
            using var context = new QuizAppContext(options);

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            // Set up controller context for model validation
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var invalidQuizViewModel = new QuizViewModel
            {
                Title = "", // Empty title
                Description = "A test quiz",
                TimeLimit = "00:30:00",
                IsActive = true,
                Questions = new List<QuestionViewModel>() // No questions
            };

            // Add model validation error
            controller.ModelState.AddModelError("Title", "Title is required");
            controller.ModelState.AddModelError("Questions", "At least one question is required");

            // Act
            var result = await controller.CreateQuiz(invalidQuizViewModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult.Value);
        }
    }
} 