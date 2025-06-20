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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuizApp.Tests
{
    public class RegisterControllerTest
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

        [Fact]
        public async Task Register_InvalidData_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<QuizAppContext>()
                .UseInMemoryDatabase(databaseName: "RegisterInvalidDataTestDb")
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

            // Test with invalid email format
            var registerModel = new RegisterViewModel
            {
                Username = "newuser",
                Email = "invalid-email", // Invalid email format
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            // Manually trigger validation
            var validationContext = new ValidationContext(registerModel);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerModel, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                    }
                }
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

            // Manually trigger validation
            var validationContext = new ValidationContext(registerModel);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(registerModel, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        controller.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
                    }
                }
            }

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            var actionResult = Assert.IsType<ActionResult<AuthResponseViewModel>>(result);
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
    }
} 