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

namespace QuizApp.Tests.Register
{
    public class RegisterDuplicateTest
    {
        [Fact]
        public async Task Register_ExistingUsername_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterDuplicateUsernameTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            // Add existing user
            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                FirstName = "Existing",
                LastName = "User",
                IsActive = true
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

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

            var registerModel = new RegisterViewModel
            {
                Username = "existinguser", // Same username as existing user
                Email = "new@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Username already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterDuplicateEmailTestDb")
                .Options;
            using var context = new QuizAppContext(options);

            // Add existing user
            var existingUser = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                FirstName = "Existing",
                LastName = "User",
                IsActive = true
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

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

            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Email = "existing@example.com", // Same email as existing user
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Email already exists", badRequestResult.Value);
        }
    }
} 