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
    public class StartGameValidTest
    {
        [Fact]
        public async Task StartGame_ValidQuizId_GameSessionCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "StartGameTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            // Add a quiz to start a game for
            var quiz = new QuizApp.Core.Models.Quiz
            {
                Title = "Test Quiz",
                Description = "Test Description",
                TimeLimit = System.TimeSpan.FromMinutes(30),
                IsActive = true
            };
            context.Quizzes.Add(quiz);
            context.SaveChanges();

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Act
            var result = await controller.StartGame(quiz.Id);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var gameSession = Assert.IsType<GameSession>(createdResult.Value);
            Assert.Equal(quiz.Id, gameSession.QuizId);

            // Verify the session is in the database
            var sessionInDb = await context.GameSessions.FindAsync(gameSession.Id);
            Assert.NotNull(sessionInDb);
        }
    }
} 