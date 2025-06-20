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
    public class JoinGameValidTest
    {
        [Fact]
        public async Task JoinGame_ValidGameCode_PlayerCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "JoinGameTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            // Add a waiting game session
            var gameSession = new GameSession
            {
                Code = "ABC123",
                Status = GameStatus.WaitingToStart,
                QuizId = 1
            };
            context.GameSessions.Add(gameSession);
            context.SaveChanges();

            // Mock the SignalR hub context (not used in this test, but required by constructor)
            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Fix: Set up a fake HttpContext so controller.User is not null
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.JoinGame("ABC123", "TestPlayer");

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var player = Assert.IsType<Player>(createdResult.Value);
            Assert.Equal("TestPlayer", player.Name);
            Assert.Equal(gameSession.Id, player.GameSessionId);
        }
    }
} 