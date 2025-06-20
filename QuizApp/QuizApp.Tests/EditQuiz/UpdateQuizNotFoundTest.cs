using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QuizApp.Tests.EditQuiz
{
    public class UpdateQuizNotFoundTest
    {
        [Fact]
        public async Task UpdateQuiz_QuizNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "UpdateQuizTestDb2")
                .Options;
            using var context = new QuizAppContext(options);

            var loggerMock = new Mock<ILogger<QuizApp.Api.Controllers.QuizController>>();
            var controller = new QuizApp.Api.Controllers.QuizController(context, loggerMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var viewModel = new QuizViewModel
            {
                Title = "Doesn't matter",
                Description = "Doesn't matter",
                TimeLimit = "00:30:00",
                IsActive = true,
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Text = "Q?",
                        Points = 100,
                        TimeLimit = 20,
                        Answers = new List<AnswerViewModel>
                        {
                            new AnswerViewModel { Text = "A", IsCorrect = true }
                        }
                    }
                }
            };

            // Act
            var result = await controller.UpdateQuiz(999, viewModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
} 