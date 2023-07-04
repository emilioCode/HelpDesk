using AutoMapper;
using HelpDesk.Controllers;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Controllers
{
    public class TraceControllerTests
    {
        private readonly Mock<ITraceService> _traceServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly TraceController _traceController;

        public TraceControllerTests()
        {
            _traceServiceMock = new Mock<ITraceService>();
            _mapperMock = new Mock<IMapper>();

            _traceController = new TraceController(_traceServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public void GetTracesById()
        {
            //Arrange
            int ticketId = 1;
            int businnesId = 1;
            var customTraces = new List<CustomTrace>();
            // Act
            _traceServiceMock.Setup(service => service.GetTracesById(ticketId, businnesId))
                .Returns(customTraces);
            _traceServiceMock.Setup(service => service.GetTracesById(ticketId, businnesId))
                .Returns(customTraces);
            var result = _traceController.Get(ticketId, businnesId) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<CustomTrace>>(response.data);
        }

        [Test]
        public async Task Post_AddTrace()
        {
            // Arrange
            var traceDto = new SeguimientoDto();
            var trace = new Seguimiento();
            // Act
            _mapperMock.Setup(x => x.Map<Seguimiento>(traceDto))
                .Returns(trace);
            _traceServiceMock.Setup(service => service.AddTrace(trace))
                .ReturnsAsync(trace);
            _mapperMock.Setup(x => x.Map<SeguimientoDto>(trace))
                .Returns(traceDto);
            var result = await _traceController.Post(traceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<SeguimientoDto>(response.data);
        }

        [Test]
        public async Task Put_UpdateTrace()
        {
            // Arrange
            var traceDto = new SeguimientoDto();
            var trace = new Seguimiento();
            // Act
            _mapperMock.Setup(x => x.Map<Seguimiento>(traceDto))
                .Returns(trace);
            _traceServiceMock.Setup(service => service.UpdateTrace(trace))
                .ReturnsAsync(trace);
            _mapperMock.Setup(x => x.Map<SeguimientoDto>(trace))
                .Returns(traceDto);
            var result = await _traceController.Put(traceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<SeguimientoDto>(response.data);
        }
    }
}
