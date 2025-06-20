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

namespace QuizApp.Tests.StartingQuiz
{
    public class StartGameInactiveQuizTest
    {
        [Fact]
        public async Task StartGame_InactiveQuiz_StillCreatesGameSession()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "StartGameTestDb4")
                .Options;
            using var context = new QuizAppContext(options);

            // Add an inactive quiz
            var quiz = new QuizApp.Core.Models.Quiz
            {
                Title = "Test Quiz",
                Description = "Test Description",
                TimeLimit = TimeSpan.FromMinutes(30),
                IsActive = false // Inactive quiz
            };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var hubContextMock = new Mock<IHubContext<QuizApp.Api.Hubs.GameHub>>();
            var controller = new QuizApp.Api.Controllers.GameController(context, hubContextMock.Object);

            // Act
            var result = await controller.StartGame(quiz.Id);

            // Assert
            // The current implementation doesn't check for inactive quizzes, so it should still create a game session
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var gameSession = Assert.IsType<GameSession>(createdResult.Value);
            Assert.Equal(quiz.Id, gameSession.QuizId);
        }
    }
} 