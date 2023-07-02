using AutoMapper;
using HelpDesk.Controllers;
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
    public class UserControllerTest
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ISecurityService> _securityServiceMock;

        private readonly UserController _userController;
        public UserControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();
            _securityServiceMock = new Mock<ISecurityService>();

            _userController = new UserController(_userServiceMock.Object, _mapperMock.Object, _securityServiceMock.Object);
        }

        [Test]
        public void UserController_Constructor_InitializesDependencies()
        {
            // Arrange

            // Act

            // Assert
            Assert.NotNull(_userController);
        }

        [Test]
        public async Task Get_ReturnsOkResultWithUserDtos()
        {
            // Arrange
            int idUser = 1;
            string option = "unique";

            var users = new List<Usuario>
            {
                new Usuario { Id = idUser, Nombre = "John" }
            };

            var userDtos = new List<UsuarioDto>
            {
                new UsuarioDto { Id = idUser, Nombre = "John" }
            };

            _userServiceMock.Setup(x => x.GetUsersByIdAndCondition(idUser, option))
                .ReturnsAsync(users);
            _mapperMock.Setup(x => x.Map<IEnumerable<UsuarioDto>>(users))
                .Returns(userDtos);

            // Act
            var result = await _userController.Get(idUser, option) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<List<UsuarioDto>>(result.Value);
        }

        [Test]
        public async Task Post_ReturnsOkResultWithObjectResponse()
        {
            // Arrange
            var usuarioDto = new UsuarioDto { Nombre = "John" };
            var user = new Usuario { Nombre = "John" };
            var expectedResponse = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = user
            };

            _mapperMock.Setup(mapper => mapper.Map<Usuario>(usuarioDto))
                       .Returns(user);

            _userServiceMock.Setup(service => service.InsertUSer(user))
                            .ReturnsAsync(user);

            // Act
            var result = await _userController.Post(usuarioDto) as OkObjectResult;
            ObjectResponse response = result.Value as ObjectResponse;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));

            Assert.IsTrue(expectedResponse.code.Equals(response.code));
            Assert.IsTrue(expectedResponse.title.Equals(response.title));
            Assert.IsTrue(expectedResponse.icon.Equals(response.icon));
            Assert.IsTrue(expectedResponse.message.Equals(response.message));
            Assert.IsTrue(expectedResponse.data.Equals(response.data));
        }

        [Test]
        public async Task Put_ReturnsOkResultWithObjectResponse()
        {
            // Arrange
            int idUserReq = 5;
            var usuarioDto = new UsuarioDto { Nombre = "John" };
            var user = new Usuario { Nombre = "John" };
            var expectedResponse = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = true
            };

            _mapperMock.Setup(mapper => mapper.Map<Usuario>(usuarioDto))
                       .Returns(user);

            _userServiceMock.Setup(service => service.UpdateUSer(user, idUserReq))
                            .ReturnsAsync(true);

            // Act
            var result = await _userController.Put(idUserReq, usuarioDto) as OkObjectResult;
            ObjectResponse response = result.Value as ObjectResponse;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));

            Assert.IsTrue(expectedResponse.code.Equals(response.code));
            Assert.IsTrue(expectedResponse.title.Equals(response.title));
            Assert.IsTrue(expectedResponse.icon.Equals(response.icon));
            Assert.IsTrue(expectedResponse.message.Equals(response.message));
            Assert.IsTrue(expectedResponse.data.Equals(response.data));
        }

        [Test]
        public void Patch_ReturnsEncryptedPassword()
        {
            // Arrange
            string password = "password123";
            string expectedEncryptedPassword = "encryptedPassword";

            // Act
            _securityServiceMock.Setup(x=> x.Encripting(password))
                .Returns(() => expectedEncryptedPassword);
            var result = _userController.Patch(password);

            // Assert
            Assert.IsTrue(expectedEncryptedPassword.Equals(result));
        }
    
    }
}