using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuizApp.Tests.StartingQuiz
{
    public class StartGameInvalidQuizTest
    {
        [Fact]
        public async Task StartGame_InvalidQuizId_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "StartGameTestDb2")
                .Options;
            using var context = new QuizAppContext(options);

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Act
            var result = await controller.StartGame(999); // Non-existent quiz ID

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Quiz not found", notFound.Value);
        }
    }
} 