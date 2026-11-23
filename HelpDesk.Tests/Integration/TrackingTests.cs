using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    // The customer follows an order from a link, without signing in: what the link shows, who may make it, and when it stops.
    public class TrackingTests
    {
        private ApiFactory _api;

        [SetUp]
        public void SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
        }

        [TearDown]
        public void TearDown() => _api.Dispose();

        private static string TokenOf(string url) => url.Substring(url.LastIndexOf("/t/", StringComparison.Ordinal) + 3);

        private async Task<(string Url, string Token)> LinkAsync(string account, int ticketId)
        {
            var client = await _api.ClientFor(account);
            var response = await client.PostAsync("/api/Tracking/Link/" + ticketId, null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, account + " " + ticketId);
            var url = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["data"]["url"];
            return (url, TokenOf(url));
        }

        private async Task<HttpResponseMessage> ViewAsync(string token) => await _api.CreateClient().GetAsync("/api/Tracking/" + token);

        [Test]
        public async Task AnAdministrator_GetsTheSameLinkEveryTime()
        {
            var first = await LinkAsync("admin", 1);
            var second = await LinkAsync("admin", 1);

            StringAssert.Contains("/t/", first.Url);
            Assert.GreaterOrEqual(first.Token.Length, 30);
            Assert.AreEqual(first.Token, second.Token);
        }

        [Test]
        public async Task TheLinkOfAnOrderOfAnotherCompany_IsRefused()
        {
            var client = await _api.ClientFor("adminb");

            var response = await client.PostAsync("/api/Tracking/Link/1", null);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task ATechnician_MakesTheLinkOfTheirOwnOrdersOnly()
        {
            var other = await _api.ClientFor("tech2");   // order 1 belongs to tech1
            Assert.AreEqual(HttpStatusCode.Forbidden, (await other.PostAsync("/api/Tracking/Link/1", null)).StatusCode);

            var own = await LinkAsync("tech1", 1);
            Assert.IsNotEmpty(own.Token);
        }

        [Test]
        public async Task ThePublicPage_NeedsNoSignIn_AndShowsOnlyWhatTheCustomerMaySee()
        {
            var link = await LinkAsync("admin", 1);
            _api.Query(db =>
            {
                var day = DateTime.Now.Date;
                db.Seguimientos.AddRange(
                    new Seguimiento { Id = 21, IdSolicitud = 1, IdEmpresa = 1, IdUsuario = 4, Texto = "Shown to the customer", Fecha = day, Hora = TimeSpan.FromHours(10), Etiquetado = true, VisibleAlCliente = true, Habilitado = true },
                    new Seguimiento { Id = 22, IdSolicitud = 1, IdEmpresa = 1, IdUsuario = 4, Texto = "Internal note", Fecha = day, Hora = TimeSpan.FromHours(11), Habilitado = true },
                    new Seguimiento { Id = 23, IdSolicitud = 1, IdEmpresa = 1, IdUsuario = 4, Texto = "Removed note", Fecha = day, Hora = TimeSpan.FromHours(12), VisibleAlCliente = true, Habilitado = false });
                db.SaveChanges();
                return 0;
            });

            var response = await ViewAsync(link.Token);
            var body = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("1", (string)json["orderNumber"]);
            Assert.AreEqual("Company A", (string)json["company"]);
            Assert.AreEqual("open", (string)json["status"]);
            Assert.AreEqual(1, (int)json["step"]);
            Assert.AreEqual(1, ((JArray)json["activities"]).Count);
            Assert.AreEqual("Shown to the customer", (string)json["activities"][0]["text"]);
            Assert.AreEqual("tech1", (string)json["activities"][0]["who"]);
            Assert.IsTrue((bool)json["activities"][0]["isSolution"]);
            StringAssert.DoesNotContain("Internal note", body);
            StringAssert.DoesNotContain("Removed note", body);
            foreach (var forbidden in new[] { "idEmpresa", "idUsuario", "idCliente", "idSolicitud", "\"id\"", "aprobadoPor", link.Token })
            {
                StringAssert.DoesNotContain(forbidden, body, forbidden);
            }
            Assert.AreEqual("no-store", response.Headers.CacheControl.ToString());
            Assert.IsTrue(response.Headers.Contains("X-Robots-Tag"));
            Assert.AreEqual("no-referrer", response.Headers.GetValues("Referrer-Policy").Single());
        }

        [TestCase("not-a-real-token-0000000000000000")]
        [TestCase("../../etc/passwd")]
        [TestCase("a")]
        public async Task ALinkThatDoesNotExist_AnswersNotFound(string token)
        {
            var response = await ViewAsync(Uri.EscapeDataString(token));

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task ARevokedLink_AnswersNotFound_AndANewOneIsMadeOnRequest()
        {
            var link = await LinkAsync("admin", 1);
            Assert.AreEqual(HttpStatusCode.OK, (await ViewAsync(link.Token)).StatusCode);
            var admin = await _api.ClientFor("admin");

            await admin.PostAsync("/api/Tracking/Revoke/1", null);
            var after = await ViewAsync(link.Token);
            var again = await LinkAsync("admin", 1);

            Assert.AreEqual(HttpStatusCode.NotFound, after.StatusCode);
            Assert.AreNotEqual(link.Token, again.Token);
            Assert.AreEqual(HttpStatusCode.OK, (await ViewAsync(again.Token)).StatusCode);
        }

        [Test]
        public async Task AnApprovedOrder_ShowsTheLastStep_AndItsLinkExpires()
        {
            var link = await LinkAsync("admin", 12);
            _api.Query(db => { var t = db.Solicitudes.Single(x => x.Id == 12); t.Estado = "Completado"; t.AprobadoPor = 2; db.SaveChanges(); return 0; });

            var view = JObject.Parse(await (await ViewAsync(link.Token)).Content.ReadAsStringAsync());
            Assert.AreEqual("approved", (string)view["status"]);
            Assert.AreEqual(4, (int)view["step"]);
            Assert.AreEqual(100, (int)view["progress"]);

            _api.Query(db => { db.EnlacesSeguimiento.Single(x => x.Token == link.Token).VenceEn = DateTime.UtcNow.AddMinutes(-1); db.SaveChanges(); return 0; });
            Assert.AreEqual(HttpStatusCode.NotFound, (await ViewAsync(link.Token)).StatusCode);
        }

        [Test]
        public async Task AnExpiredLink_WorksAgain_IfTheOrderIsReopened()
        {
            var link = await LinkAsync("admin", 13);
            _api.Query(db => { db.EnlacesSeguimiento.Single(x => x.Token == link.Token).VenceEn = DateTime.UtcNow.AddMinutes(-1); db.SaveChanges(); return 0; });

            Assert.AreEqual(HttpStatusCode.OK, (await ViewAsync(link.Token)).StatusCode, "the order is not approved, so the end date does not apply");
        }

        [Test]
        public async Task ApprovingAnOrder_StartsTheCountdownOfItsLink()
        {
            var link = await LinkAsync("admin", 11);
            Assert.IsNull(_api.Query(db => db.EnlacesSeguimiento.Single(x => x.Token == link.Token).VenceEn));

            using (var scope = _api.Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<ITrackingService>().OnApprovedAsync(11);
            }

            var ends = _api.Query(db => db.EnlacesSeguimiento.Single(x => x.Token == link.Token).VenceEn);
            Assert.NotNull(ends);
            Assert.That(ends.Value, Is.EqualTo(DateTime.UtcNow.AddDays(30)).Within(TimeSpan.FromMinutes(5)));
        }

        [Test]
        public async Task TheEmail_CarriesTheLinkOfTheOrder()
        {
            var admin = await _api.ClientFor("admin");

            var response = await admin.PostAsync("/api/Tracking/Email/1", null);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("1", (string)JObject.Parse(body)["code"]);
            var token = TokenOf((string)JObject.Parse(body)["data"]["url"]);
            var sent = _api.Mail.Sent.Single(m => m.Subject.Contains("No. 1"));
            Assert.AreEqual("ca@test.local", sent.To);
            StringAssert.Contains("/t/" + token, sent.Body);
        }

        [Test]
        public async Task ACustomerWithoutAnAddress_IsToldAndNothingIsSent()
        {
            _api.Query(db => { db.Clientes.Single(c => c.Id == 3).Correo = ""; db.SaveChanges(); return 0; });
            _api.Query(db => { db.Solicitudes.Single(t => t.Id == 5).IdCliente = 3; db.SaveChanges(); return 0; });
            var before = _api.Mail.Sent.Count;
            var admin = await _api.ClientFor("admin");

            var response = await admin.PostAsync("/api/Tracking/Email/5", null);

            Assert.AreEqual("5", (string)JObject.Parse(await response.Content.ReadAsStringAsync())["code"]);
            Assert.AreEqual(before, _api.Mail.Sent.Count);
        }

        [Test]
        public async Task TheOrderReport_PrintsTheLinkAsAQrCode()
        {
            var client = _api.CreateClient();
            await client.PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "admin", contrasena = TestUsers.Password }));

            var response = await client.GetAsync("/OrderReport?value=1-1");
            var html = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.Contains("qr-follow", html);
            StringAssert.Contains("<svg", html);
            var token = _api.Query(db => db.EnlacesSeguimiento.Single(l => l.IdSolicitud == 1).Token);
            StringAssert.Contains("/t/" + token, html);
        }

        [Test]
        public async Task ANewActivity_IsHiddenFromTheCustomer_UntilTheTechnicianShowsIt()
        {
            var tech = await _api.ClientFor("tech1");
            var created = await tech.PostAsync("/api/Trace", ApiFactory.Json(new { idSolicitud = 1, idEmpresa = 1, idUsuario = 4, texto = "Checked the power supply" }));
            var id = (int)JObject.Parse(await created.Content.ReadAsStringAsync())["data"]["id"];
            var link = await LinkAsync("tech1", 1);
            Assert.AreEqual(0, ((JArray)JObject.Parse(await (await ViewAsync(link.Token)).Content.ReadAsStringAsync())["activities"]).Count(a => (string)a["text"] == "Checked the power supply"));

            await tech.PostAsync("/api/Trace/Visible/" + id + "/true", null);
            // Marking it as a favourite afterwards must not hide it again.
            await tech.PostAsync("/api/Trace/Put", ApiFactory.Json(new { id, idSolicitud = 1, texto = "Checked the power supply", favorito = true, etiquetado = false, habilitado = true }));

            var activities = (JArray)JObject.Parse(await (await ViewAsync(link.Token)).Content.ReadAsStringAsync())["activities"];
            Assert.AreEqual(1, activities.Count(a => (string)a["text"] == "Checked the power supply"));
        }

        [Test]
        public async Task AnotherTechnician_CannotChangeWhatTheCustomerSees()
        {
            var tech1 = await _api.ClientFor("tech1");
            var id = (int)JObject.Parse(await (await tech1.PostAsync("/api/Trace", ApiFactory.Json(new { idSolicitud = 1, idEmpresa = 1, idUsuario = 4, texto = "Mine" }))).Content.ReadAsStringAsync())["data"]["id"];
            var tech2 = await _api.ClientFor("tech2");

            var response = await tech2.PostAsync("/api/Trace/Visible/" + id + "/true", null);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task TheStaffEndpoints_NeedASignIn()
        {
            var client = _api.CreateClient();
            foreach (var url in new[] { "/api/Tracking/Link/1", "/api/Tracking/Email/1", "/api/Tracking/Revoke/1", "/api/Trace/Visible/1/true" })
            {
                Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.PostAsync(url, null)).StatusCode, url);
            }
        }

        [Test]
        public async Task TooManyCalls_FromOneAddress_AreStopped()
        {
            using var limited = new ApiFactory(enforce: true, trackingRequestsPerMinute: 3);
            limited.Seed();
            var client = limited.CreateClient();

            var answers = new System.Collections.Generic.List<HttpStatusCode>();
            for (var i = 0; i < 5; i++) answers.Add((await client.GetAsync("/api/Tracking/not-a-real-token-0000000000000000")).StatusCode);

            CollectionAssert.AreEqual(new[] { HttpStatusCode.NotFound, HttpStatusCode.NotFound, HttpStatusCode.NotFound, HttpStatusCode.TooManyRequests, HttpStatusCode.TooManyRequests }, answers);
        }
    }
}
