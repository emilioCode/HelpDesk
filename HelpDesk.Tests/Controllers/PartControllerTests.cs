using AutoMapper;
using HelpDesk.Controllers;
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
    public class PartControllerTests
    {
        private readonly Mock<IPieceService> _pieceServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly PartController _partController;
        public PartControllerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _pieceServiceMock = new Mock<IPieceService>();

            _partController = new PartController(_pieceServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Get_GetPieces()
        {
            // Arrange
            int ticketId = 0;
            int businessId = 0;
            var parts = new List<Piezas>();
            var partsDto = new List<PiezasDto>();
            // Act
            _pieceServiceMock.Setup(service => service.GetPieces(ticketId, businessId))
                .Returns(parts);
            _mapperMock.Setup(x => x.Map<IEnumerable<PiezasDto>>(parts))
                .Returns(partsDto);
            var result = _partController.Get(ticketId, businessId) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<PiezasDto>>(result.Value);
        }

        [Test]
        public async Task Post_AddPiece()
        {
            // Arrange
            var pieceDto = new PiezasDto();
            var piece = new Piezas();
            //Act
            _mapperMock.Setup(x => x.Map<Piezas>(pieceDto))
                .Returns(piece);
            _pieceServiceMock.Setup(service => service.AddPiece(piece))
                .ReturnsAsync(piece);
            _mapperMock.Setup(x => x.Map<PiezasDto>(piece))
                .Returns(pieceDto);
            var result = await _partController.Post(pieceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<PiezasDto>(response.data);
        }

        [Test]
        public async Task Put_UpdatePiece()
        {
            // Arrange
            var pieceDto = new PiezasDto();
            var piece = new Piezas();
            //Act
            _mapperMock.Setup(x => x.Map<Piezas>(pieceDto))
                .Returns(piece);
            _pieceServiceMock.Setup(service => service.UpdatePiece(piece))
                .ReturnsAsync(piece);
            _mapperMock.Setup(x => x.Map<PiezasDto>(piece))
                .Returns(pieceDto);
            var result = await _partController.Put(pieceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<PiezasDto>(response.data);
        }

        [Test]
        public async Task Delete_DeletePiece()
        {
            // Arrange
            var pieceId = 0;
            //Act
            _pieceServiceMock.Setup(service => service.DeletePiece(pieceId))
                .ReturnsAsync(true);
            var result = await _partController.Delete(pieceId) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            //Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<bool>(response.data);
        }
    }
}
