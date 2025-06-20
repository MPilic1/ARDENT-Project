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
    public class RegisterValidDataTest
    {
        [Fact]
        public async Task Register_ValidData_ReturnsUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterTestDb")
                .Options;
            using var context = new QuizAppContext(options);

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
                Email = "newuser@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            var response = Assert.IsType<AuthResponseViewModel>(actionResult.Value);
            Assert.Equal(registerModel.Username, response.Username);
            Assert.Equal(registerModel.Email, response.Email);
            Assert.Equal(registerModel.FirstName, response.FirstName);
            Assert.Equal(registerModel.LastName, response.LastName);

            // Verify user was saved to database
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Username == registerModel.Username);
            Assert.NotNull(savedUser);
            Assert.True(savedUser.IsActive);
            Assert.NotNull(savedUser.PasswordHash);
            Assert.NotNull(savedUser.PasswordSalt);
        }
    }
} 