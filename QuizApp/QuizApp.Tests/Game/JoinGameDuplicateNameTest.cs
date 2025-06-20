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
using System.Linq;

namespace QuizApp.Tests.Game
{
    public class JoinGameDuplicateNameTest
    {
        [Fact]
        public async Task JoinGame_DuplicatePlayerName_StillCreatesPlayer()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "JoinGameTestDb5")
                .Options;
            using var context = new QuizAppContext(options);

            // Add a game session
            var gameSession = new GameSession
            {
                Code = "ABC123",
                Status = GameStatus.WaitingToStart,
                QuizId = 1
            };
            context.GameSessions.Add(gameSession);

            // Add an existing player
            context.Players.Add(new Player { GameSessionId = gameSession.Id, Name = "TestPlayer" });
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.JoinGame("ABC123", "TestPlayer"); // Try to join with same name

            // Assert
            // The current implementation doesn't check for duplicate names, so it should create a new player
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var player = Assert.IsType<Player>(createdResult.Value);
            Assert.Equal("TestPlayer", player.Name);
            Assert.Equal(gameSession.Id, player.GameSessionId);
            
            // Verify that both players exist
            var players = await context.Players.Where(p => p.GameSessionId == gameSession.Id && p.Name == "TestPlayer").ToListAsync();
            Assert.Equal(2, players.Count);
        }
    }
} 