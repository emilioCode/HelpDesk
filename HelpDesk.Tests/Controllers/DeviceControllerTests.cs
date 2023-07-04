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
    public class DeviceControllerTests
    {
        private readonly Mock<IDeviceService> _deviceServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly DeviceController _deviceController;
        public DeviceControllerTests()
        {
            _deviceServiceMock = new Mock<IDeviceService>();
            _mapperMock = new Mock<IMapper>();

            _deviceController = new DeviceController(_deviceServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public void Get_GetDevicesByTicketId()
        {
            // Arrange
            int ticketId = 0;
            List<Equipo> devices = new List<Equipo>();
            List<EquipoDto> devicesDto = new List<EquipoDto>();
            // Act
            _deviceServiceMock.Setup(service => service.GetDevicesByTicketId(ticketId))
                .Returns(devices);
            _mapperMock.Setup(x => x.Map<IEnumerable<EquipoDto>>(devices))
                .Returns(devicesDto);
            var result = _deviceController.Get(ticketId) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<EquipoDto>>(result.Value);
        }

        [Test]
        public async Task PostOne_AddDevice()
        {
            // Arrange
            var device = new Equipo();
            var deviceDto = new EquipoDto();
            // Act
            _mapperMock.Setup(x => x.Map<Equipo>(deviceDto))
                .Returns(device);
            _deviceServiceMock.Setup(service => service.AddDevice(device))
                .ReturnsAsync(device);
            _mapperMock.Setup(x => x.Map<EquipoDto>(device))
                .Returns(deviceDto);
            var result = await _deviceController.PostOne(deviceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<EquipoDto>(response.data);
        }

        [Test]
        public async Task PostArray_AddRangeDevice()
        {
            // Arrange
            var devices = new List<Equipo>();
            var devicesDto = new List<EquipoDto>();
            // Act
            _mapperMock.Setup(x => x.Map<List<Equipo>>(devicesDto))
                .Returns(devices);
            _deviceServiceMock.Setup(service => service.AddRangeDevice(devices))
                .ReturnsAsync(devices);
            _mapperMock.Setup(x => x.Map<List<EquipoDto>>(devices))
                .Returns(devicesDto);
            var result = await _deviceController.PostArray(devicesDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<EquipoDto>>(response.data);
        }

        [Test]
        public async Task Put_ModifyDevice()
        {
            // Arrange
            var device = new Equipo();
            var deviceDto = new EquipoDto();
            // Act
            _mapperMock.Setup(x => x.Map<Equipo>(deviceDto))
                .Returns(device);
            _deviceServiceMock.Setup(service => service.ModifyDevice(device))
                .ReturnsAsync(device);
            _mapperMock.Setup(x => x.Map<EquipoDto>(device))
                .Returns(deviceDto);
            var result = await _deviceController.Put(deviceDto) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<EquipoDto>(response.data);
        }

        [Test]
        public async Task Delete_DeleteDevide()
        {
            // Arrange
            int id = 0;
            // Act
            _deviceServiceMock.Setup(service => service.DeleteDevide(id))
                .ReturnsAsync(true);
            var result = await _deviceController.Delete(id) as OkObjectResult;
            var response = result.Value as ObjectResponse;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<bool>(response.data);
        }
    }
}
