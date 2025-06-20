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
using System;

namespace QuizApp.Tests.Game
{
    public class JoinGameAlreadyStartedTest
    {
        [Fact]
        public async Task JoinGame_GameAlreadyStarted_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "JoinGameTestDb3")
                .Options;
            using var context = new QuizAppContext(options);

            // Add a started game session
            var gameSession = new GameSession
            {
                Code = "ABC123",
                Status = GameStatus.InProgress,
                QuizId = 1,
                StartTime = DateTime.UtcNow
            };
            context.GameSessions.Add(gameSession);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.JoinGame("ABC123", "TestPlayer");

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Game session not found or already started", notFound.Value);
        }
    }
} 