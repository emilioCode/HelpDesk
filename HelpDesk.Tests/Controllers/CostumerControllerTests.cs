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
    public class CostumerControllerTests
    {
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly CostumerController _customerController;
        public CostumerControllerTests()
        {
            _customerServiceMock = new Mock<ICustomerService> ();
            _mapperMock = new Mock<IMapper> ();

            _customerController = new CostumerController(_customerServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public async Task Get_GetCustomersByIdAndCondition()
        {
            // Arrange
            int idUserOridClient = 0;
            string option = "";
            List<Cliente> customers = new List<Cliente>();
            List<ClienteDto> customersDto = new List<ClienteDto>();
            // Act
            _customerServiceMock.Setup(service => service.GetCustomersByIdAndCondition(idUserOridClient, option))
                .ReturnsAsync(customers);
            _mapperMock.Setup(x => x.Map<IEnumerable<ClienteDto>>(customers))
                .Returns(customersDto);
            var result = await _customerController.Get(idUserOridClient, option) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<ClienteDto>>(result.Value);
        }

        [Test]
        public async Task Post_CreateCustomer()
        {
            // Arrange
            var customerDto = new ClienteDto();
            var customer = new Cliente();
            // Act
            _mapperMock.Setup(x => x.Map<Cliente>(customerDto))
                .Returns(() => customer);
            _customerServiceMock.Setup(service => service.CreateCustomer(customer))
                .ReturnsAsync(customer);
            var result = await _customerController.Post(customerDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<Cliente>(response.data);
            Assert.IsTrue(customer.Equals(response.data));
        }

        [Test]
        public async Task Put_UpdateCustomer()
        {
            // Arrange
            int userCreatorId = 0;
            var customerDto = new ClienteDto();
            var customer = new Cliente();
            // Act
            _mapperMock.Setup(x => x.Map<Cliente>(customerDto))
                .Returns(customer);
            _customerServiceMock.Setup(service => service.UpdateCustomer(customer, userCreatorId))
                .ReturnsAsync(true);
            var result = await _customerController.Put(userCreatorId, customerDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<bool>(response.data);
            Assert.IsTrue(response.data as bool?);
        }
    }
}
