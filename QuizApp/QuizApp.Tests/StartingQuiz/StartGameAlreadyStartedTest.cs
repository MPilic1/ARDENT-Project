using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace QuizApp.Tests.StartingQuiz
{
    public class StartGameAlreadyStartedTest
    {
        [Fact]
        public async Task StartGame_AlreadyStarted_StillCreatesNewGameSession()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "StartGameTestDb3")
                .Options;
            using var context = new QuizAppContext(options);

            // Add a quiz
            var quiz = new QuizApp.Core.Models.Quiz
            {
                Title = "Test Quiz",
                Description = "Test Description",
                TimeLimit = TimeSpan.FromMinutes(30),
                IsActive = true
            };
            context.Quizzes.Add(quiz);

            // Add an existing game session for this quiz
            var existingSession = new GameSession
            {
                QuizId = quiz.Id,
                Status = GameStatus.InProgress,
                StartTime = DateTime.UtcNow,
                Code = "ABC123"
            };
            context.GameSessions.Add(existingSession);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Act
            var result = await controller.StartGame(quiz.Id);

            // Assert
            // The current implementation doesn't check for existing game sessions, so it should create a new one
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var gameSession = Assert.IsType<GameSession>(createdResult.Value);
            Assert.Equal(quiz.Id, gameSession.QuizId);
            
            // Verify that both sessions exist
            var sessions = await context.GameSessions.Where(gs => gs.QuizId == quiz.Id).ToListAsync();
            Assert.Equal(2, sessions.Count);
        }
    }
} 