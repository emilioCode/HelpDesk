using HelpDesk.Controllers;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Controllers
{
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly Mock<HelpDeskDBContext> _context;

        private readonly EventController _eventController;
        public EventControllerTests()
        {
            _context = new Mock<HelpDeskDBContext>();
            _eventServiceMock = new Mock<IEventService>();

            _eventController = new EventController(_context.Object, _eventServiceMock.Object);
        }

        [Test]
        public async Task Post_GetEvent()
        {
            // Arrange
            var user = new Mock<Usuario>();
            var events = new List<Event>();
            // Act
            _eventServiceMock.Setup(service => service.GetEvent(user.Object.Id))
                .ReturnsAsync(events);
            var result = await _eventController.Post(user.Object) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<ObjectResponse>(response);
            Assert.IsAssignableFrom<List<Event>>(response.data);
            Assert.IsTrue(events.Equals(response.data as List<Event>));
        }
    }
}
