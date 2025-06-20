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
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace QuizApp.Tests.Register
{
    public class RegisterValidationTest
    {
        [Fact]
        public async Task Register_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterInvalidEmailTestDb")
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
                Email = "invalid-email", // Invalid email format
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            // Validate model manually since we're not going through the MVC pipeline
            var validationContext = new ValidationContext(registerModel);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerModel, validationContext, validationResults, true);

            if (!isValid)
            {
                controller.ModelState.AddModelError("Email", "Invalid email format");
            }

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task Register_WeakPassword_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterWeakPasswordTestDb")
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
                Password = "weak", // Weak password
                FirstName = "New",
                LastName = "User"
            };

            // Validate model manually since we're not going through the MVC pipeline
            var validationContext = new ValidationContext(registerModel);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerModel, validationContext, validationResults, true);

            if (!isValid)
            {
                controller.ModelState.AddModelError("Password", "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one digit");
            }

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
    }
} 