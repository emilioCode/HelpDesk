using HelpDesk.Core.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    public class SessionRenewalTests
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

        private static HttpClient WithToken(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static string RoleIn(string token)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token).Claims.First(c => c.Type == ClaimTypes.Role).Value;
        }

        [Test]
        public async Task Refresh_WithAValidToken_ReturnsANewTokenAndRenewsTheCookie()
        {
            var client = _api.CreateClient();
            var login = await client.PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "tech1", contrasena = TestUsers.Password }));
            var first = (string)JObject.Parse(await login.Content.ReadAsStringAsync())["token"];
            await Task.Delay(1100);   // tokens issued in the same second would be identical except for their id

            var response = await WithToken(_api.CreateClient(), first).PostAsync("/api/Login/Refresh", null);
            var body = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var renewed = (string)body["token"];
            Assert.IsNotEmpty(renewed);
            Assert.AreNotEqual(first, renewed);
            Assert.AreEqual("TECNICO", (string)body["acceso"]);
            Assert.Greater((DateTime)body["expiresAt"], DateTime.UtcNow.AddMinutes(58), "a full new lifetime");
            StringAssert.Contains("hd_token=" + renewed, string.Join(";", response.Headers.GetValues("Set-Cookie")));
        }

        [Test]
        public async Task TheNewToken_WorksOnTheApi()
        {
            var first = await _api.LoginAsync("tech1");
            var response = await WithToken(_api.CreateClient(), first).PostAsync("/api/Login/Refresh", null);
            var renewed = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["token"];

            var call = await WithToken(_api.CreateClient(), renewed).GetAsync("/api/User/4/unique");

            Assert.AreEqual(HttpStatusCode.OK, call.StatusCode);
        }

        [Test]
        public async Task Refresh_WithoutAToken_IsRejected()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login/Refresh", null);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Refresh_WithAnAlteredToken_IsRejected()
        {
            var token = await _api.LoginAsync("tech1");
            var response = await WithToken(_api.CreateClient(), token + "x").PostAsync("/api/Login/Refresh", null);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Refresh_WithATokenWhoseRoleChanged_IsRefused_AndSigningInGivesTheCurrentRole()
        {
            var token = await _api.LoginAsync("renewrole");
            Assert.AreEqual("TECNICO", RoleIn(token));

            _api.Query(db => { var u = db.Usuarios.Single(x => x.Id == 12); u.Acceso = "MODERADOR"; db.SaveChanges(); return 0; });
            var response = await WithToken(_api.CreateClient(), token).PostAsync("/api/Login/Refresh", null);
            var again = await _api.LoginAsync("renewrole");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "the old token no longer says what the user is, so it is not renewed");
            Assert.AreEqual("MODERADOR", RoleIn(again), "signing in again carries the current role");
        }

        [Test]
        public async Task Refresh_ForAUserWhoWasDisabled_IsRejected()
        {
            var token = await _api.LoginAsync("renewoff");
            _api.Query(db => { var u = db.Usuarios.Single(x => x.Id == 13); u.Habilitado = false; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).PostAsync("/api/Login/Refresh", null);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.IsNull(await _api.LoginAsync("renewoff"), "and they cannot sign in again");
        }

        [Test]
        public async Task Refresh_ForAUserOfADisabledCompany_IsRejected()
        {
            var token = await _api.LoginAsync("adminb");
            _api.Query(db => { var e = db.Empresas.Single(x => x.Id == 2); e.Habilitado = false; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).PostAsync("/api/Login/Refresh", null);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            _api.Query(db => { var e = db.Empresas.Single(x => x.Id == 2); e.Habilitado = true; db.SaveChanges(); return 0; });
        }
    }

    public class DefaultSettingsTests
    {
        [Test]
        public async Task WithoutAnyAuthSetting_RequestsNeedAToken()
        {
            using var api = new ApiFactory(enforce: null);
            api.Seed();

            var response = await api.CreateClient().GetAsync("/api/User/4/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task TheTokenLasts60MinutesByDefault()
        {
            using var api = new ApiFactory(enforce: null);
            api.Seed();

            var login = await api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "tech1", contrasena = TestUsers.Password }));
            var expires = (DateTime)JObject.Parse(await login.Content.ReadAsStringAsync())["expiresAt"];

            Assert.Greater(expires, DateTime.UtcNow.AddMinutes(58));
            Assert.Less(expires, DateTime.UtcNow.AddMinutes(62));
        }
    }
}

namespace HelpDesk.Tests.Integration
{
    // A token stays valid until it expires, but a user who was disabled or deleted meanwhile is refused.
    public class DisabledUserTokenTests
    {
        private ApiFactory _api;
        private ApiFactory _cached;

        [OneTimeSetUp]
        public void SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
            _cached = new ApiFactory(enforce: true, activeCheckSeconds: 60);
            _cached.Seed();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _api.Dispose();
            _cached.Dispose();
        }

        private static HttpClient WithToken(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        [Test]
        public async Task AnActiveUser_KeepsWorking()
        {
            var token = await _api.LoginAsync("tech1");

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/4/unique");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task AUserDisabledAfterSigningIn_IsRefusedWithTheSameToken()
        {
            var token = await _api.LoginAsync("revokeoff");
            Assert.AreEqual(HttpStatusCode.OK, (await WithToken(_api.CreateClient(), token).GetAsync("/api/User/14/unique")).StatusCode);
            _api.Query(db => { db.Usuarios.Single(x => x.Id == 14).Habilitado = false; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/14/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task AUserDeletedAfterSigningIn_IsRefusedWithTheSameToken()
        {
            var token = await _api.LoginAsync("revokedel");
            _api.Query(db => { db.Usuarios.Remove(db.Usuarios.Single(x => x.Id == 15)); db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/15/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task TheAnswerIsRemembered_SoADisabledUserIsRefusedOnlyAfterTheCacheWindow()
        {
            var token = await _cached.LoginAsync("revokecache");
            Assert.AreEqual(HttpStatusCode.OK, (await WithToken(_cached.CreateClient(), token).GetAsync("/api/User/16/unique")).StatusCode);
            _cached.Query(db => { db.Usuarios.Single(x => x.Id == 16).Habilitado = false; db.SaveChanges(); return 0; });

            var response = await WithToken(_cached.CreateClient(), token).GetAsync("/api/User/16/unique");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "inside the window the remembered answer is used (this is the trade-off for not querying on every call)");
        }

        [Test]
        public async Task AUserWhoseRoleChanged_IsRefusedWithTheSameToken_UntilTheySignInAgain()
        {
            var token = await _api.LoginAsync("rolechg");
            Assert.AreEqual(HttpStatusCode.OK, (await WithToken(_api.CreateClient(), token).GetAsync("/api/User/17/unique")).StatusCode);
            _api.Query(db => { db.Usuarios.Single(x => x.Id == 17).Acceso = "MODERADOR"; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/17/unique");
            var again = await _api.LoginAsync("rolechg");
            var afterSigningIn = await WithToken(_api.CreateClient(), again).GetAsync("/api/User/17/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, afterSigningIn.StatusCode);
        }

        [Test]
        public async Task AUserMovedToAnotherCompany_IsRefusedWithTheSameToken()
        {
            var token = await _api.LoginAsync("companychg");
            Assert.AreEqual(HttpStatusCode.OK, (await WithToken(_api.CreateClient(), token).GetAsync("/api/User/18/unique")).StatusCode);
            _api.Query(db => { db.Usuarios.Single(x => x.Id == 18).IdEmpresa = 2; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/18/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task AUserEditedWithoutChangingRoleOrCompany_KeepsWorking()
        {
            var token = await _api.LoginAsync("samerole");
            _api.Query(db => { db.Usuarios.Single(x => x.Id == 19).Nombre = "New name"; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).GetAsync("/api/User/19/unique");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task TheReportCookie_OfADisabledUser_IsRefusedToo()
        {
            var token = await _api.LoginAsync("tech2");
            _api.Query(db => { db.Usuarios.Single(x => x.Id == 5).Habilitado = false; db.SaveChanges(); return 0; });
            var client = _api.CreateClient();
            client.DefaultRequestHeaders.Add("Cookie", "hd_token=" + token);

            var response = await client.GetAsync("/OrderReport?value=1-1");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
