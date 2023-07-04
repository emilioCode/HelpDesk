using AutoMapper;
using HelpDesk.Controllers;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Controllers
{
    public class LoginControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly LoginController _loginController;

        public LoginControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();  

            _loginController = new LoginController(_userServiceMock.Object, _mapperMock.Object);
        }

        [Test]
        public void LoginController_Constructor_InitializesDependencies()
        {
            // Arrange

            // Act

            // Assert
            Assert.NotNull(_loginController);
        }

        [Test]
        public async Task Get_ValidateUserAccountWithObjectResponse()
        {
            // Arrange
            int idEmpresa = 5;
            string userAccount = "";
            bool validationResult = true;
            
            // Act
            _userServiceMock.Setup(service => service.ValidateUserAccount(idEmpresa, userAccount))
                .ReturnsAsync(validationResult);

            var result = await _loginController.Get(idEmpresa, userAccount) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsAssignableFrom<bool>(result.Value);
        }

        [Test]
        public void Post_GetLoginByCredentialsWithObjectResponse()
        {
            // Arrange
            UserLogin userLogin = new UserLogin();
            Usuario user = new Usuario();
            UserIdentity userIdentity = new UserIdentity();

            // Act
            _mapperMock.Setup(service => service.Map<Usuario>(userLogin))
                .Returns(user);
            _userServiceMock.Setup(service => service.GetLoginByCredentials(user))
                .Returns(userIdentity);

            var result = _loginController.Post(userLogin) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.StatusCode.Equals(200));
            Assert.IsTrue(userIdentity.Equals(result.Value));
        }
    }
}
