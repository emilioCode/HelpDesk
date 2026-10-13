using HelpDesk.Controllers;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Controllers
{
    public class LoginControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private Mock<ITokenService> _tokenServiceMock;
        private LoginController _loginController;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenServiceMock = new Mock<ITokenService>();

            _loginController = new LoginController(_userServiceMock.Object, _tokenServiceMock.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        [Test]
        public void LoginController_Constructor_InitializesDependencies()
        {
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
        public async Task Post_ValidCredentials_ReturnsTheUserWithTheToken()
        {
            // Arrange
            var userLogin = new UserLogin { CuentaUsuario = "ana", Contrasena = "secret1" };
            var userIdentity = new UserIdentity { Id = 2, UserName = "ana" };
            var expires = DateTime.UtcNow.AddHours(8);
            _userServiceMock.Setup(service => service.LoginAsync("ana", "secret1")).ReturnsAsync(userIdentity);
            _tokenServiceMock.Setup(service => service.Create(userIdentity)).Returns(("the-token", expires));

            // Act
            var result = await _loginController.Post(userLogin) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var returned = result.Value as UserIdentity;
            Assert.AreEqual("the-token", returned.Token);
            Assert.AreEqual(expires, returned.ExpiresAt);
        }

        [Test]
        public async Task Post_ValidCredentials_SetsAnHttpOnlyCookieForTheReports()
        {
            // Arrange
            var userLogin = new UserLogin { CuentaUsuario = "ana", Contrasena = "secret1" };
            var userIdentity = new UserIdentity { Id = 2 };
            _userServiceMock.Setup(service => service.LoginAsync("ana", "secret1")).ReturnsAsync(userIdentity);
            _tokenServiceMock.Setup(service => service.Create(userIdentity)).Returns(("the-token", DateTime.UtcNow.AddHours(8)));

            // Act
            await _loginController.Post(userLogin);

            // Assert
            var setCookie = _loginController.Response.Headers["Set-Cookie"].ToString();
            StringAssert.Contains("hd_token=the-token", setCookie);
            StringAssert.Contains("httponly", setCookie.ToLowerInvariant());
            StringAssert.Contains("samesite=strict", setCookie.ToLowerInvariant());
        }

        [Test]
        public async Task Post_InvalidCredentials_ReturnsNoUserAndNoToken()
        {
            // Arrange
            var userLogin = new UserLogin { CuentaUsuario = "ana", Contrasena = "wrong" };
            _userServiceMock.Setup(service => service.LoginAsync("ana", "wrong")).ReturnsAsync((UserIdentity)null);

            // Act
            var result = await _loginController.Post(userLogin) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsNull(result.Value);
            _tokenServiceMock.Verify(service => service.Create(It.IsAny<UserIdentity>()), Times.Never);
            Assert.IsFalse(_loginController.Response.Headers.ContainsKey("Set-Cookie"));
        }

        [Test]
        public void Logout_ClearsTheReportCookie()
        {
            // Act
            _loginController.Logout();

            // Assert
            StringAssert.Contains("hd_token=;", _loginController.Response.Headers["Set-Cookie"].ToString());
        }
    }
}
