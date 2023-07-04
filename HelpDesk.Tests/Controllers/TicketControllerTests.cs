using AutoMapper;
using HelpDesk.Controllers;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Controllers
{
    public class TicketControllerTests
    {
        private readonly Mock<ITicketService> _ticketServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMailService> _mailServiceMock;

        private readonly TicketController _ticketController;
        public TicketControllerTests()
        {
            _ticketServiceMock = new Mock<ITicketService>();
            _mapperMock = new Mock<IMapper>();
            _mailServiceMock = new Mock<IMailService>();

            _ticketController = new TicketController(_ticketServiceMock.Object, _mapperMock.Object, _mailServiceMock.Object);
        }

        [Test]
        public async Task Get_GetTicketsByIdAndCondition()
        {
            // Arrange
            int userId = 0;
            string option = "unique";
            var requests = new List<SolicitudT>();
            // Act
            _ticketServiceMock.Setup(service => service.GetTicketsByIdAndCondition(userId, option))
                .ReturnsAsync(requests);
            var result = await _ticketController.Get(userId, option) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<SolicitudT>>(result.Value);
        }

        [Test]
        public async Task Post_CreateTicket_SendEmail()
        {
            // Arrange
            var ticketDto = new SolicitudDto();
            var ticket = new Solicitud();

            var email = new EmailMessage();
            var tupleResponse = (ticket, email);
            // Act
            _mapperMock.Setup(x => x.Map<Solicitud>(ticketDto))
                .Returns(ticket);
            _ticketServiceMock.Setup(service => service.CreateTicket(ticket))
                .ReturnsAsync(tupleResponse);
            var result = await _ticketController.Post(ticketDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Solicitud>(response.data);
        }

        [Test]
        public async Task Post_CreateTicket_WithoutSendEmail()
        {
            // Arrange
            var ticketDto = new SolicitudDto();
            var ticket = new Solicitud();

            var email = new EmailMessage();
            email = null;
            var tupleResponse = (ticket, email);
            // Act
            _mapperMock.Setup(x => x.Map<Solicitud>(ticketDto))
                .Returns(ticket);
            _ticketServiceMock.Setup(service => service.CreateTicket(ticket))
                .ReturnsAsync(tupleResponse);
            var result = await _ticketController.Post(ticketDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Solicitud>(response.data);
        }

        [Test]
        public async Task EDIT_UpdateTicket_SendEmail()
        {
            // Arrange
            var ticketDto = new SolicitudDto();
            var ticket = new Solicitud();
            var toDo = string.Empty;
            var email = new EmailMessage();
            var tupleResponse = (ticket, email);
            // Act
            _mapperMock.Setup(x => x.Map<Solicitud>(ticketDto))
                .Returns(ticket);
            _ticketServiceMock.Setup(service => service.UpdateTicket(ticket, toDo))
                .ReturnsAsync(tupleResponse);
            var result = await _ticketController.EDIT(toDo, ticketDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Solicitud>(response.data);
        }

        [Test]
        public async Task EDIT_UpdateTicket_WithoutSendEmail()
        {
            // Arrange
            var ticketDto = new SolicitudDto();
            var ticket = new Solicitud();
            var toDo = string.Empty;
            var email = new EmailMessage();
            email = null;
            var tupleResponse = (ticket, email);
            // Act
            _mapperMock.Setup(x => x.Map<Solicitud>(ticketDto))
                .Returns(ticket);
            _ticketServiceMock.Setup(service => service.UpdateTicket(ticket, toDo))
                .ReturnsAsync(tupleResponse);
            var result = await _ticketController.EDIT(toDo, ticketDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Solicitud>(response.data);
        }

        [Test]
        public async Task GetMetricsByUserId()
        {
            // Arrange
            int userId = 1;
            var metrics = new Metric();
            // Act
            _ticketServiceMock.Setup(service => service.GetMetricsByUserId(userId))
                .ReturnsAsync(metrics);
            var result = await _ticketController.numbersOfTickets(userId) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Metric>(response.data);
        }

        [Test]
        public void GetJsonTicket()
        {
            // Arrange
            var ticketLiteDto = new SolicitudLiteDto();
            var ticket = new Solicitud();
            var responseExpected = new List<SummaryInformation>();
            // Act
            _mapperMock.Setup(x => x.Map<Solicitud>(ticketLiteDto))
                .Returns(ticket);
            _ticketServiceMock.Setup(service => service.GetSummaryInformation(ticket))
                .Returns(responseExpected);
            var result = _ticketController.GetJsonTicket(ticketLiteDto) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<SummaryInformation>>(result.Value);
        }
    }
}
