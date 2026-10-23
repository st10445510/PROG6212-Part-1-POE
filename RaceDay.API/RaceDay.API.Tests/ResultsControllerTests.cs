
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Tests
{
    [TestClass]
    public class ResultsControllerTests
    {
        [TestMethod]
        public async Task CreateResult_WhenEnrolmentDoesNotExist_ReturnsBadRequest()
        {
            // Arrange: create a temporary database
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayContext(options);

            var controller = new ResultsController(context);

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "6"),
                    new Claim(ClaimTypes.Role, "Organiser")
                },
                "TestAuthentication");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            var request = new CreateResultRequest
            {
                EnrolmentId = 999,
                FinishTime = new TimeOnly(1, 0, 0),
                FinishingPosition = 1
            };

            // Act
            var response = await controller.CreateResult(request);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(response);
        }
        [TestMethod]
        public async Task GetResult_WhenResultDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<RaceDayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new RaceDayContext(options);

            var controller = new ResultsController(context);

            // Act
            var response = await controller.GetResult(999);

            // Assert
            Assert.IsInstanceOfType<NotFoundObjectResult>(response);
        }
    }
}
