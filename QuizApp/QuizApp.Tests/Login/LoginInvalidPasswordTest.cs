using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Api.Controllers;
using QuizApp.Infrastructure.Data;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace QuizApp.Tests.Login
{
    public class LoginInvalidPasswordTest
    {
        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "LoginTestDb2")
                .Options;
            using var context = new QuizAppContext(options);

            var salt = QuizApp.Core.Security.PasswordHashProvider.GetSalt();
            var hash = QuizApp.Core.Security.PasswordHashProvider.GetHash("CorrectPassword", salt);

            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordSalt = salt,
                PasswordHash = hash,
                IsActive = true,
                FirstName = "Test",
                LastName = "User"
            };
            context.Users.Add(user);
            context.SaveChanges();

            var loggerMock = new Mock<ILogger<AuthController>>();
            var configMock = new Mock<IConfiguration>();

            var controller = new AuthController(context, configMock.Object, loggerMock.Object)
            {
                SkipSignInForTests = true
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Login(new LoginViewModel
            {
                Username = "testuser",
                Password = "WrongPassword"
            });

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Invalid username or password", unauthorized.Value);
        }
    }
} 