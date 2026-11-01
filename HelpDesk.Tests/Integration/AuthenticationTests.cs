using HelpDesk.Security;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    public class AuthenticationTests
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

        [TestCase("GET", "/api/User/1/unique")]
        [TestCase("GET", "/api/User/1/*")]
        [TestCase("GET", "/api/Business/1")]
        [TestCase("GET", "/api/Ticket/1/*")]
        [TestCase("GET", "/api/Ticket/numbersOfTickets/1")]
        [TestCase("GET", "/api/Costumer/1/*")]
        [TestCase("GET", "/api/Device/1")]
        [TestCase("GET", "/api/Part/1/1")]
        [TestCase("GET", "/api/Trace/1/1")]
        [TestCase("GET", "/api/Login/1/someone")]
        [TestCase("GET", "/OrderReport?value=1-1")]
        [TestCase("POST", "/hub/negotiate?negotiateVersion=1")]
        public async Task Anonymous_Requests_AreRejected(string method, string url)
        {
            var response = await _api.CreateClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, url);
        }

        [Test]
        public async Task Anonymous_PostToEvent_IsRejected()
        {
            var response = await _api.CreateClient().PostAsync("/api/Event", ApiFactory.Json(new { id = 1 }));
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestCase("/")]
        [TestCase("/ticket")]
        [TestCase("/user")]
        [TestCase("/main.js")]
        public async Task TheSinglePageApp_CanBeLoadedWithoutASession(string url)
        {
            var response = await _api.CreateClient().GetAsync(url);
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode, "the sign-in screen has to load: " + url);
        }

        [Test]
        public async Task Login_WithTheRightPassword_ReturnsTheUserAndAToken()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "admin", contrasena = TestUsers.Password }));
            var body = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("ADMINISTRADOR", (string)body["acceso"]);
            Assert.AreEqual(1, (int)body["idEmpresa"]);
            Assert.IsNotEmpty((string)body["token"]);
            Assert.IsNotNull(body["expiresAt"]);
            Assert.IsNull(body["contrasena"], "the sign-in response never includes a password");
        }

        [Test]
        public async Task Login_WithAWrongPassword_ReturnsNoContentAndNoCookie()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "admin", contrasena = "wrong-password" }));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.IsFalse(response.Headers.Contains("Set-Cookie"));
        }

        [Test]
        public async Task Login_ForAnUnknownAccount_ReturnsNoContent()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "nobody", contrasena = TestUsers.Password }));
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Login_SetsAnHttpOnlyCookieForTheReports()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "admin", contrasena = TestUsers.Password }));
            var cookie = string.Join(";", response.Headers.GetValues("Set-Cookie")).ToLowerInvariant();

            StringAssert.Contains("hd_token=", cookie);
            StringAssert.Contains("httponly", cookie);
            StringAssert.Contains("samesite=strict", cookie);
        }

        [Test]
        public async Task AValidToken_AuthorizesRequests()
        {
            var client = await _api.ClientFor("admin");
            var response = await client.GetAsync("/api/User/2/unique");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task ALegacyPlainTextPassword_IsHashedTheFirstTimeTheUserSignsIn()
        {
            Assert.AreEqual("legacy123", _api.Query(db => db.Usuarios.Single(u => u.Id == 7).Contrasena));

            Assert.IsNotNull(await _api.LoginAsync("legacy", "legacy123"));

            var stored = _api.Query(db => db.Usuarios.Single(u => u.Id == 7).Contrasena);
            Assert.AreNotEqual("legacy123", stored);
            Assert.IsTrue(stored.StartsWith("$2") && stored.Length == 60);
            Assert.IsNotNull(await _api.LoginAsync("legacy", "legacy123"), "the same password still works afterwards");
            Assert.IsNull(await _api.LoginAsync("legacy", "legacy12"));
        }

        [Test]
        public async Task TheAccount_IsLockedAfterFiveFailedAttempts_EvenWithTheRightPassword()
        {
            for (var i = 0; i < 5; i++)
            {
                Assert.IsNull(await _api.LoginAsync("lockme", "wrong-" + i));
            }

            var response = await _api.CreateClient().PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = "lockme", contrasena = TestUsers.Password }));

            Assert.AreEqual((HttpStatusCode)429, response.StatusCode);
            StringAssert.Contains("intentos", await response.Content.ReadAsStringAsync());
        }

        [Test]
        public async Task ATokenSignedWithAnotherKey_IsRejected()
        {
            var token = CreateToken("a-completely-different-signing-key-0123456789", DateTime.UtcNow.AddHours(1));
            Assert.AreEqual(HttpStatusCode.Unauthorized, await StatusWith(token));
        }

        [Test]
        public async Task AnExpiredToken_IsRejected()
        {
            var token = CreateToken(ApiFactory.JwtKey, DateTime.UtcNow.AddHours(-1));
            Assert.AreEqual(HttpStatusCode.Unauthorized, await StatusWith(token));
        }

        [Test]
        public async Task ATokenWithAlteredClaims_IsRejected()
        {
            var good = await _api.LoginAsync("tech1");
            var parts = good.Split('.');
            var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1])).Replace("TECNICO", "ROOT");
            var forged = parts[0] + "." + Base64UrlEncoder.Encode(payload) + "." + parts[2];

            Assert.AreEqual(HttpStatusCode.Unauthorized, await StatusWith(forged));
        }

        [Test]
        public async Task Garbage_InTheAuthorizationHeader_IsRejected()
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, await StatusWith("not-a-token"));
        }

        [Test]
        public async Task TheHub_AcceptsATokenInTheQueryString()
        {
            var token = await _api.LoginAsync("admin");
            var response = await _api.CreateClient().PostAsync("/hub/negotiate?negotiateVersion=1&access_token=" + token, null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Logout_ClearsTheCookie_WithoutNeedingAToken()
        {
            var response = await _api.CreateClient().PostAsync("/api/Login/Logout", null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.Contains("hd_token=;", string.Join(";", response.Headers.GetValues("Set-Cookie")));
        }

        private async Task<HttpStatusCode> StatusWith(string token)
        {
            var client = _api.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return (await client.GetAsync("/api/User/4/unique")).StatusCode;
        }

        private static string CreateToken(string key, DateTime expires)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "4"),
                new Claim(ClaimTypes.Role, "TECNICO"),
                new Claim(JwtTokenService.CompanyClaim, "1")
            };
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("HelpDesk", "HelpDesk", claims, expires.AddHours(-2), expires, credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class TransitionSwitchTests
    {
        [Test]
        public async Task WithEnforceOff_AnonymousRequestsStillWork()
        {
            using var api = new ApiFactory(enforce: false);
            api.Seed();

            var response = await api.CreateClient().GetAsync("/api/User/4/unique");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task WithEnforceOff_ATokenStillTakesPrecedenceOverTheUrl()
        {
            using var api = new ApiFactory(enforce: false);
            api.Seed();
            var client = await api.ClientFor("tech1");

            var users = JArray.Parse(await client.GetStringAsync("/api/User/1/unique"));

            Assert.AreEqual(4, (int)users[0]["id"], "the user comes from the token, not from the URL");
        }
    }
}
