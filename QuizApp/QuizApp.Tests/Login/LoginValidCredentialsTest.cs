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
    public class LoginValidCredentialsTest
    {
        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "LoginTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            var password = "TestPassword123!";
            var salt = QuizApp.Core.Security.PasswordHashProvider.GetSalt();
            var hash = QuizApp.Core.Security.PasswordHashProvider.GetHash(password, salt);

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
                Password = password
            });

            // Assert
            if (result.Result != null)
            {
                var errorResult = result.Result as ObjectResult;
                var errorMsg = errorResult?.Value?.ToString() ?? "Unknown error";
                Assert.Fail($"Login failed: {errorMsg}");
            }

            Assert.NotNull(result.Value);
            var response = Assert.IsType<AuthResponseViewModel>(result.Value);
            Assert.Equal("testuser", response.Username);
            Assert.Equal("test@example.com", response.Email);
        }
    }
} 