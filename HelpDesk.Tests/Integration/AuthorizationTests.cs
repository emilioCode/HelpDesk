using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    public class AuthorizationTests
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

        private static object NewUser(string account, string role = "TECNICO", int company = 1, string password = "Secret123") => new
        {
            nombre = "Name " + account, numDocumento = "123", cuentaUsuario = account, contrasena = password,
            acceso = role, correo = account + "@test.local", idEmpresa = company, habilitado = true
        };

        private static int[] Ids(string json) => JArray.Parse(json).Select(x => (int)x["id"]).OrderBy(x => x).ToArray();

        // ---------- identity comes from the token ----------

        [Test]
        public async Task TheUserIdInTheUrl_IsIgnored_ForUsers()
        {
            var tech = await _api.ClientFor("tech1");

            var asSelf = JArray.Parse(await tech.GetStringAsync("/api/User/1/unique"));
            var all = JArray.Parse(await tech.GetStringAsync("/api/User/1/*"));

            Assert.AreEqual(4, (int)asSelf[0]["id"]);
            Assert.AreEqual(0, all.Count, "a technician cannot list users, whatever id the URL carries");
        }

        [Test]
        public async Task TheUserIdInTheUrl_IsIgnored_ForTickets()
        {
            var tech = await _api.ClientFor("tech1");

            var tickets = Ids(await tech.GetStringAsync("/api/Ticket/5/unique"));

            CollectionAssert.IsSubsetOf(new[] { 1, 4, 5 }, tickets, "their own tickets are listed");
            CollectionAssert.DoesNotContain(tickets, 2, "a ticket of another technician is not");
            CollectionAssert.DoesNotContain(tickets, 3, "nor is one of another company");
        }

        [Test]
        public async Task ManagersSeeTheirOwnCompanyOnly()
        {
            var adminA = await _api.ClientFor("admin");
            var adminB = await _api.ClientFor("adminb");

            var seenByA = Ids(await adminA.GetStringAsync("/api/Ticket/1/*"));
            CollectionAssert.IsSubsetOf(new[] { 1, 2, 4, 5 }, seenByA);
            CollectionAssert.DoesNotContain(seenByA, 3);
            Assert.AreEqual(new[] { 3 }, Ids(await adminB.GetStringAsync("/api/Ticket/6/*")));
        }

        [Test]
        public async Task Business_ShowsOnlyTheUsersCompany_ToNonRootUsers()
        {
            var adminB = await _api.ClientFor("adminb");

            var businesses = JArray.Parse(await adminB.GetStringAsync("/api/Business/1"));

            Assert.AreEqual(1, businesses.Count);
            Assert.AreEqual(2, (int)businesses[0]["id"]);
        }

        // ---------- passwords are never returned ----------

        [Test]
        public async Task Responses_NeverContainPasswords()
        {
            var admin = await _api.ClientFor("admin");

            var list = await admin.GetStringAsync("/api/User/2/*");
            var business = await admin.GetStringAsync("/api/Business/2");
            var created = await (await admin.PostAsync("/api/User", ApiFactory.Json(NewUser("nopass1")))).Content.ReadAsStringAsync();

            foreach (var body in new[] { list, business, created })
            {
                StringAssert.DoesNotContain("$2a$", body);
                StringAssert.DoesNotContain(TestUsers.Password, body);
                StringAssert.DoesNotContain("Secret123", body);
                StringAssert.DoesNotContain("mail-secret", body);
                Assert.IsFalse(JToken.Parse(body).SelectTokens("$..contrasena").Any(t => t.Type != JTokenType.Null));
            }
        }

        // ---------- user management ----------

        [Test]
        public async Task ModeratorsAndTechnicians_CannotCreateUsers()
        {
            foreach (var account in new[] { "mod", "tech1" })
            {
                var client = await _api.ClientFor(account);
                var response = await client.PostAsync("/api/User", ApiFactory.Json(NewUser("blocked-" + account)));
                Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, account);
            }
            Assert.IsFalse(_api.Query(db => db.Usuarios.Any(u => u.CuentaUsuario.StartsWith("blocked-"))));
        }

        [Test]
        public async Task AnAdministrator_CreatesUsersInTheirOwnCompany_EvenIfTheRequestAsksForAnother()
        {
            var admin = await _api.ClientFor("admin");

            var response = await admin.PostAsync("/api/User", ApiFactory.Json(NewUser("newtech", company: 2)));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var created = _api.Query(db => db.Usuarios.Single(u => u.CuentaUsuario == "newtech"));
            Assert.AreEqual(1, created.IdEmpresa);
            Assert.AreNotEqual("Secret123", created.Contrasena);
            Assert.IsTrue(created.Contrasena.StartsWith("$2"));
            Assert.IsNotNull(await _api.LoginAsync("newtech", "Secret123"));
        }

        [Test]
        public async Task AnAdministrator_CannotGrantARoleAboveTheirOwn()
        {
            var admin = await _api.ClientFor("admin");

            var response = await admin.PostAsync("/api/User", ApiFactory.Json(NewUser("wannaberoot", role: "ROOT")));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsFalse(_api.Query(db => db.Usuarios.Any(u => u.CuentaUsuario == "wannaberoot")));
        }

        [Test]
        public async Task ATechnician_CannotRaiseTheirOwnRoleOrMoveCompany()
        {
            var tech = await _api.ClientFor("tech2");
            var body = new { id = 5, nombre = "Tech Two Renamed", numDocumento = "000", cuentaUsuario = "tech2", acceso = "ROOT", correo = "tech2@test.local", idEmpresa = 2, habilitado = true };

            var response = await tech.PostAsync("/api/User/Put/5", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var stored = _api.Query(db => db.Usuarios.Single(u => u.Id == 5));
            Assert.AreEqual("Tech Two Renamed", stored.Nombre, "their own profile data can be edited");
            Assert.AreEqual("TECNICO", stored.Acceso);
            Assert.AreEqual(1, stored.IdEmpresa);
        }

        [Test]
        public async Task EditingAProfileWithoutAPassword_KeepsTheCurrentOne()
        {
            var tech = await _api.ClientFor("tech3");
            var body = new { id = 9, nombre = "Tech Three", numDocumento = "000", cuentaUsuario = "tech3", acceso = "TECNICO", correo = "tech3@test.local", idEmpresa = 1, habilitado = true };

            var response = await tech.PostAsync("/api/User/Put/9", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(await _api.LoginAsync("tech3"), "the previous password still works");
        }

        [Test]
        public async Task ChangingThePassword_StoresANewHash()
        {
            var user = await _api.ClientFor("pwuser");
            var body = new { id = 10, nombre = "pwuser", numDocumento = "000", cuentaUsuario = "pwuser", contrasena = "BrandNew123", acceso = "TECNICO", correo = "pwuser@test.local", idEmpresa = 1, habilitado = true };

            await user.PostAsync("/api/User/Put/10", ApiFactory.Json(body));

            Assert.IsNotNull(await _api.LoginAsync("pwuser", "BrandNew123"));
            Assert.IsNull(await _api.LoginAsync("pwuser", TestUsers.Password));
            Assert.IsTrue(_api.Query(db => db.Usuarios.Single(u => u.Id == 10).Contrasena).StartsWith("$2"));
        }

        [Test]
        public async Task ATechnician_CannotEditAnotherUser()
        {
            var tech = await _api.ClientFor("tech2");
            var body = new { id = 3, nombre = "Hacked", numDocumento = "000", cuentaUsuario = "mod", acceso = "MODERADOR", correo = "mod@test.local", idEmpresa = 1, habilitado = true };

            var response = await tech.PostAsync("/api/User/Put/5", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("mod", _api.Query(db => db.Usuarios.Single(u => u.Id == 3).Nombre));
        }

        [Test]
        public async Task AnAdministrator_CannotEditAUserOfAnotherCompany()
        {
            var admin = await _api.ClientFor("admin");
            var body = new { id = 6, nombre = "Hacked", numDocumento = "000", cuentaUsuario = "adminb", acceso = "ADMINISTRADOR", correo = "adminb@test.local", idEmpresa = 2, habilitado = true };

            var response = await admin.PostAsync("/api/User/Put/2", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("adminb", _api.Query(db => db.Usuarios.Single(u => u.Id == 6).Nombre));
        }

        [Test]
        public async Task OnlyAdministrators_CanCheckIfAnAccountExists()
        {
            var tech = await _api.ClientFor("tech2");
            var admin = await _api.ClientFor("admin");

            Assert.AreEqual(HttpStatusCode.Forbidden, (await tech.GetAsync("/api/Login/1/admin")).StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync("/api/Login/1/admin")).StatusCode);
        }

        // ---------- company settings ----------

        [Test]
        public async Task ATechnician_CannotUpdateTheCompany()
        {
            var tech = await _api.ClientFor("tech2");
            var body = new { id = 1, razonSocial = "Hacked Inc", secuenciaticket = "3" };

            var response = await tech.PostAsync("/api/Business/Put/5", ApiFactory.Json(body));

            Assert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Company A", _api.Query(db => db.Empresas.Single(e => e.Id == 1).RazonSocial));
        }

        [Test]
        public async Task AnAdministrator_CannotUpdateAnotherCompany()
        {
            var admin = await _api.ClientFor("admin");
            var body = new { id = 2, razonSocial = "Hacked Inc", secuenciaticket = "2" };

            var response = await admin.PostAsync("/api/Business/Put/2", ApiFactory.Json(body));

            Assert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Company B", _api.Query(db => db.Empresas.Single(e => e.Id == 2).RazonSocial));
        }

        [Test]
        public async Task TheCompanyMailPassword_IsStoredEncrypted_AndKeptWhenNotSent()
        {
            var admin = await _api.ClientFor("adminc");

            await admin.PostAsync("/api/Business/Put/11", ApiFactory.Json(new { id = 3, razonSocial = "Company C", secuenciaticket = "2", correo = "c@test.local", contrasena = "new-mail-pass" }));
            var encrypted = _api.Query(db => db.Empresas.Single(e => e.Id == 3).Contrasena);
            Assert.IsTrue(encrypted.StartsWith("enc:v1:"));
            StringAssert.DoesNotContain("new-mail-pass", encrypted);

            await admin.PostAsync("/api/Business/Put/11", ApiFactory.Json(new { id = 3, razonSocial = "Company C renamed", secuenciaticket = "2", correo = "c@test.local" }));
            var after = _api.Query(db => db.Empresas.Single(e => e.Id == 3));
            Assert.AreEqual("Company C renamed", after.RazonSocial);
            Assert.AreEqual(encrypted, after.Contrasena, "without a new password the stored one is kept");
        }

        [Test]
        public async Task OnlyRoot_ManagesTheUserLimitOfACompany()
        {
            var admin = await _api.ClientFor("adminc");

            await admin.PostAsync("/api/Business/Put/11", ApiFactory.Json(new { id = 3, razonSocial = "Company C", secuenciaticket = "2", limit = 999 }));

            Assert.AreEqual(20, _api.Query(db => db.Empresas.Single(e => e.Id == 3).Limit));
        }

        // ---------- company isolation ----------

        [Test]
        public async Task TicketDetails_OfAnotherCompany_AreForbidden()
        {
            var adminA = await _api.ClientFor("admin");

            Assert.AreEqual(HttpStatusCode.OK, (await adminA.GetAsync("/api/Device/1")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await adminA.GetAsync("/api/Device/3")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await adminA.GetAsync("/api/Part/3/2")).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await adminA.GetAsync("/api/Trace/3/2")).StatusCode);
        }

        [Test]
        public async Task AskingForMyTicketsWithTheOtherCompanyId_StillReturnsMyCompany()
        {
            var adminA = await _api.ClientFor("admin");

            var parts = JArray.Parse(await adminA.GetStringAsync("/api/Part/1/2"));

            Assert.AreEqual(1, parts.Count);
            Assert.AreEqual("P1", (string)parts[0]["noSerial"]);
        }

        [Test]
        public async Task ACustomer_OfAnotherCompany_IsForbidden()
        {
            var adminA = await _api.ClientFor("admin");

            Assert.AreEqual(HttpStatusCode.Forbidden, (await adminA.GetAsync("/api/Costumer/2/unique")).StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, (await adminA.GetAsync("/api/Costumer/1/unique")).StatusCode);
        }

        [Test]
        public async Task ATicket_OfAnotherCompany_CannotBeEdited()
        {
            var adminA = await _api.ClientFor("admin");
            var body = new { id = 3, noSecuencia = "1", idEmpresa = 2, idUsuario = 6, idCliente = 2, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto", descripcion = "Hacked" };

            var response = await adminA.PostAsync("/api/Ticket/EDIT/EDITAR", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("Ticket 3", _api.Query(db => db.Solicitudes.Single(t => t.Id == 3).Descripcion));
        }

        [Test]
        public async Task ADevice_CannotBeAddedToATicketOfAnotherCompany()
        {
            var adminA = await _api.ClientFor("admin");
            var device = new { idSolicitud = 3, idEmpresa = 2, descripcion = "Spy", fallaReportada = "x", marca = "x", modelo = "x", noSerial = "SPY" };

            var response = await adminA.PostAsync("/api/Device/PostOne", ApiFactory.Json(device));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsFalse(_api.Query(db => db.Equipos.Any(e => e.NoSerial == "SPY")));
        }

        [Test]
        public async Task ANewDevice_IsStoredWithTheCompanyOfTheUser()
        {
            var adminA = await _api.ClientFor("admin");
            var device = new { idSolicitud = 1, idEmpresa = 2, descripcion = "Mine", fallaReportada = "x", marca = "x", modelo = "x", noSerial = "MINE1" };

            var response = await adminA.PostAsync("/api/Device/PostOne", ApiFactory.Json(device));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _api.Query(db => db.Equipos.Single(e => e.NoSerial == "MINE1").IdEmpresa));
        }

        [Test]
        public async Task ANewActivity_IsStoredUnderTheSignedInUser()
        {
            var tech = await _api.ClientFor("tech2");
            var activity = new { idSolicitud = 2, idEmpresa = 1, idUsuario = 6, texto = "Doing it" };

            var response = await tech.PostAsync("/api/Trace", ApiFactory.Json(activity));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(5, _api.Query(db => db.Seguimientos.Single(s => s.Texto == "Doing it").IdUsuario));
        }

        // ---------- ticket rules ----------

        [Test]
        public async Task ATechnician_CannotAssignOrApproveTickets()
        {
            var tech = await _api.ClientFor("tech2");
            var assign = new { id = 2, noSecuencia = "2", idEmpresa = 1, idUsuario = 9, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto" };
            var approve = new { id = 2, noSecuencia = "2", idEmpresa = 1, idUsuario = 5, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Completado" };

            Assert.AreEqual(HttpStatusCode.Forbidden, (await tech.PostAsync("/api/Ticket/EDIT/USUARIO", ApiFactory.Json(assign))).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await tech.PostAsync("/api/Ticket/EDIT/APROBAR", ApiFactory.Json(approve))).StatusCode);
            Assert.AreEqual(5, _api.Query(db => db.Solicitudes.Single(t => t.Id == 2).IdUsuario));
        }

        [Test]
        public async Task ATechnician_CanOnlyWorkOnTheirOwnTickets()
        {
            var tech1 = await _api.ClientFor("tech1");
            var others = new { id = 2, noSecuencia = "2", idEmpresa = 1, idUsuario = 5, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "en Proceso" };
            var mine = new { id = 10, noSecuencia = "10", idEmpresa = 1, idUsuario = 4, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "en Proceso" };

            Assert.AreEqual(HttpStatusCode.Forbidden, (await tech1.PostAsync("/api/Ticket/EDIT/ESTADO", ApiFactory.Json(others))).StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, (await tech1.PostAsync("/api/Ticket/EDIT/ESTADO", ApiFactory.Json(mine))).StatusCode);
            Assert.AreEqual("en Proceso", _api.Query(db => db.Solicitudes.Single(t => t.Id == 10).Estado));
            Assert.AreEqual("Abierto", _api.Query(db => db.Solicitudes.Single(t => t.Id == 2).Estado));
        }

        [Test]
        public async Task EditingATicket_CannotReassignItOrChangeItsNumber()
        {
            var tech1 = await _api.ClientFor("tech1");
            var body = new { id = 13, noSecuencia = "999", idEmpresa = 2, idUsuario = 9, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "en Proceso" };

            await tech1.PostAsync("/api/Ticket/EDIT/ESTADO", ApiFactory.Json(body));

            var stored = _api.Query(db => db.Solicitudes.Single(t => t.Id == 13));
            Assert.AreEqual("13", stored.NoSecuencia);
            Assert.AreEqual(1, stored.IdEmpresa);
            Assert.AreEqual(4, stored.IdUsuario);
        }

        [Test]
        public async Task AModerator_CanAssignATicket()
        {
            var mod = await _api.ClientFor("mod");
            var body = new { id = 11, noSecuencia = "11", idEmpresa = 1, idUsuario = 9, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto" };

            var response = await mod.PostAsync("/api/Ticket/EDIT/USUARIO", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
            Assert.AreEqual(9, _api.Query(db => db.Solicitudes.Single(t => t.Id == 11).IdUsuario));
        }

        [Test]
        public async Task AModerator_CannotAssignATicketToAUserOfAnotherCompany()
        {
            var mod = await _api.ClientFor("mod");
            var body = new { id = 1, noSecuencia = "1", idEmpresa = 1, idUsuario = 6, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto" };

            var response = await mod.PostAsync("/api/Ticket/EDIT/USUARIO", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual(4, _api.Query(db => db.Solicitudes.Single(t => t.Id == 1).IdUsuario));
        }

        [Test]
        public async Task TheApprover_IsTheSignedInUser_NotWhoeverTheRequestSays()
        {
            var mod = await _api.ClientFor("mod");
            var body = new { id = 12, noSecuencia = "12", idEmpresa = 1, idUsuario = 4, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Completado", aprobadoPor = 99 };

            var response = await mod.PostAsync("/api/Ticket/EDIT/APROBAR", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
            Assert.AreEqual(3, _api.Query(db => db.Solicitudes.Single(t => t.Id == 12).AprobadoPor));
        }

        [Test]
        public async Task ANewTicket_BelongsToTheCompanyOfTheUser_EvenIfTheRequestAsksForAnother()
        {
            var admin = await _api.ClientFor("admin");
            var body = new { idEmpresa = 2, idCliente = 1, idUsuario = 0, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto", descripcion = "Brand new" };

            var response = await admin.PostAsync("/api/Ticket", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, _api.Query(db => db.Solicitudes.Single(t => t.Descripcion == "Brand new").IdEmpresa));
        }

        [Test]
        public async Task ATicket_CannotBeCreatedForACustomerOfAnotherCompany()
        {
            var admin = await _api.ClientFor("admin");
            var body = new { idEmpresa = 1, idCliente = 2, idUsuario = 0, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto", descripcion = "Cross company" };

            var response = await admin.PostAsync("/api/Ticket", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.IsFalse(_api.Query(db => db.Solicitudes.Any(t => t.Descripcion == "Cross company")));
        }

        [Test]
        public async Task ATicket_CannotStartBeforeItWasCreated()
        {
            var tech = await _api.ClientFor("tech1");
            var body = new { id = 1, noSecuencia = "1", idEmpresa = 1, idUsuario = 4, idCliente = 1, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Abierto", fechaInicio = DateTime.Now.Date.AddDays(-3) };

            var response = await tech.PostAsync("/api/Ticket/EDIT/EDITAR", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- customers ----------

        [Test]
        public async Task OnlyAdministrators_CanDisableACustomer()
        {
            var mod = await _api.ClientFor("mod");
            var admin = await _api.ClientFor("admin");
            var edit = new { id = 3, nombre = "Customer A2 edited", correo = "ca2@test.local", idEmpresa = 1, habilitado = true };
            var disable = new { id = 3, nombre = "Customer A2 edited", correo = "ca2@test.local", idEmpresa = 1, habilitado = false };

            Assert.AreEqual(HttpStatusCode.OK, (await mod.PostAsync("/api/Costumer/Put/3", ApiFactory.Json(edit))).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await mod.PostAsync("/api/Costumer/Put/3", ApiFactory.Json(disable))).StatusCode);
            Assert.AreEqual(true, _api.Query(db => db.Clientes.Single(c => c.Id == 3).Habilitado));
            Assert.AreEqual(HttpStatusCode.OK, (await admin.PostAsync("/api/Costumer/Put/2", ApiFactory.Json(disable))).StatusCode);
            Assert.AreEqual(false, _api.Query(db => db.Clientes.Single(c => c.Id == 3).Habilitado));
        }

        [Test]
        public async Task ACustomer_OfAnotherCompany_CannotBeEdited()
        {
            var admin = await _api.ClientFor("admin");
            var body = new { id = 2, nombre = "Hacked", correo = "cb@test.local", idEmpresa = 2, habilitado = true };

            var response = await admin.PostAsync("/api/Costumer/Put/2", ApiFactory.Json(body));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.AreEqual("Customer B", _api.Query(db => db.Clientes.Single(c => c.Id == 2).Nombre));
        }
    }
}
