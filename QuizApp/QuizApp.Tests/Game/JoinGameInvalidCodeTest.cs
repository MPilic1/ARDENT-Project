using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace QuizApp.Tests.Game
{
    public class JoinGameInvalidCodeTest
    {
        [Fact]
        public async Task JoinGame_InvalidGameCode_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "JoinGameTestDb2")
                .Options;
            using var context = new QuizAppContext(options);

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Fix: Set up a fake HttpContext so controller.User is not null
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.JoinGame("WRONGCODE", "TestPlayer");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
} 