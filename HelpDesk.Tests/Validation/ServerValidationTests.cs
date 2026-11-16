using HelpDesk.Core.DTOs;
using HelpDesk.Infrastructure.Validators;
using HelpDesk.Tests.Integration;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Validation
{
    // The rules of the validators are applied to the bodies the API receives, and they do not stand in the way of what the screens do.
    [TestFixture]
    public class ServerValidationTests
    {
        private ApiFactory _api;

        [OneTimeSetUp]
        public void SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
        }

        [OneTimeTearDown]
        public void TearDown() => _api.Dispose();

        // ---- the rules themselves

        [Test]
        public void TheRootUser_NeedsNoDocument_ButAnybodyElseDoes()
        {
            var validator = new UserValidator();
            var root = new UsuarioDto { Id = 1, Nombre = "Root", CuentaUsuario = "root", Acceso = "ROOT", IdEmpresa = 1, NumDocumento = null };
            Assert.IsTrue(validator.Validate(root).IsValid, "root has no document in the initial data");
            var technician = new UsuarioDto { Id = 4, Nombre = "Tech", CuentaUsuario = "tech", Acceso = "TECNICO", IdEmpresa = 1, NumDocumento = null };
            Assert.IsFalse(validator.Validate(technician).IsValid, "a technician without document is refused");
            root.NumDocumento = new string('1', 31);
            Assert.IsFalse(validator.Validate(root).IsValid, "and the length limit still applies to root");
        }

        // ---- the rules at work on the requests

        [Test]
        public async Task ACustomerWithoutAName_OrWithoutACompany_IsRefused_AndNothingIsSaved()
        {
            var admin = await _api.ClientFor("admin");
            var before = await admin.GetStringAsync("/api/Costumer/2/*");

            var noName = await admin.PostAsync("/api/Costumer", ApiFactory.Json(new { rnc = "1", idEmpresa = 1 }));
            Assert.AreEqual(HttpStatusCode.BadRequest, noName.StatusCode);
            Assert.That(await noName.Content.ReadAsStringAsync(), Does.Contain("Nombre"), "the answer says which field");

            var noCompany = await admin.PostAsync("/api/Costumer", ApiFactory.Json(new { nombre = "Customer X", rnc = "1", idEmpresa = 0 }));
            Assert.AreEqual(HttpStatusCode.BadRequest, noCompany.StatusCode);
            Assert.That(await noCompany.Content.ReadAsStringAsync(), Does.Contain("IdEmpresa"));

            Assert.AreEqual(before, await admin.GetStringAsync("/api/Costumer/2/*"), "and no customer was created");
        }

        [Test]
        public async Task APartWithAQuantityOfZero_IsRefused()
        {
            var tech = await _api.ClientFor("tech1");
            var response = await tech.PostAsync("/api/Part", ApiFactory.Json(new { idSolicitud = 1, idEmpresa = 1, cantidad = 0, noSerial = "S1", descripcion = "Cable" }));
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task RootCanSaveItsOwnRecord_WithoutADocument()
        {
            // root has its own (legacy) password in the test data
            var root = _api.CreateClient();
            root.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _api.LoginAsync("root", "root"));
            var response = await root.PostAsync("/api/User/Put/1", ApiFactory.Json(new { id = 1, nombre = "root", cuentaUsuario = "root", acceso = "ROOT", idEmpresa = 1, habilitado = true }));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
