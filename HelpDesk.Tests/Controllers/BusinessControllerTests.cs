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
    public class BusinessControllerTests
    {
        private readonly Mock<IBusinessService> _businessService;
        private readonly Mock<IMapper> _mapperMock;

        private readonly BusinessController _businessController;
        public BusinessControllerTests()
        {
            _businessService = new Mock<IBusinessService>();
            _mapperMock = new Mock<IMapper>();

            _businessController = new BusinessController(_businessService.Object, _mapperMock.Object);
        }

        [Test]
        public async Task Get_GetBusinessesByUserAccess()
        {
            // Arrange
            int userId = 0;
            var businesses = new List<Empresa>();
            var businessDtos = new List<EmpresaDto>();
            // Act
            _businessService.Setup(service => service.GetBusinessesByUserAccess(userId))
                .ReturnsAsync(businesses);
            _mapperMock.Setup(x => x.Map<IEnumerable<EmpresaDto>>(businesses))
                .Returns(businessDtos);
            var result = await _businessController.Get(userId) as OkObjectResult;
            var response =  result.Value as List<EmpresaDto>;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsTrue(businessDtos.Equals(response));
        }

        [Test]
        public async Task Post_CreateBusinness()
        {
            // Arrange
            int userId = 0;
            var businessDto = new EmpresaDto();
            var business = new Empresa();
            // Act
            _mapperMock.Setup(x =>x.Map<Empresa>(businessDto))
                .Returns(() => business);
            _businessService.Setup(service => service.CreateBusinness(userId, business))
                .ReturnsAsync(business);
            var result = await _businessController.Post(userId, businessDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Empresa>(response.data);
            Assert.IsTrue(business.Equals(response.data));
        }

        [Test]
        public async Task Put_UpdateBusiness()
        {
            // Arrange
            int userId = 0;
            var businessDto = new EmpresaDto();
            var business = new Empresa();
            // Act
            _mapperMock.Setup(x => x.Map<Empresa>(businessDto))
                .Returns(business);
            _businessService.Setup(service => service.UpdateBusiness(userId, business))
                 .ReturnsAsync(true);
            var result = await _businessController.Put(userId, businessDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<bool>(response.data);
        }
    }
}
