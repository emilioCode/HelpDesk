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
        public async Task Refresh_ReadsTheRoleAgain()
        {
            var token = await _api.LoginAsync("renewrole");
            Assert.AreEqual("TECNICO", RoleIn(token));

            _api.Query(db => { var u = db.Usuarios.Single(x => x.Id == 12); u.Acceso = "MODERADOR"; db.SaveChanges(); return 0; });
            var response = await WithToken(_api.CreateClient(), token).PostAsync("/api/Login/Refresh", null);
            var renewed = (string)JObject.Parse(await response.Content.ReadAsStringAsync())["token"];

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("MODERADOR", RoleIn(renewed), "the new token carries the current role, not the one at sign-in");
        }

        [Test]
        public async Task Refresh_ForAUserWhoWasDisabled_IsRejectedAndClearsTheCookie()
        {
            var token = await _api.LoginAsync("renewoff");
            _api.Query(db => { var u = db.Usuarios.Single(x => x.Id == 13); u.Habilitado = false; db.SaveChanges(); return 0; });

            var response = await WithToken(_api.CreateClient(), token).PostAsync("/api/Login/Refresh", null);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            StringAssert.Contains("hd_token=;", string.Join(";", response.Headers.GetValues("Set-Cookie")));
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
