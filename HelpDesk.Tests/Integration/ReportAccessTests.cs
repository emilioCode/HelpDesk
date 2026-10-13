using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    public class ReportAccessTests
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

        private async Task<HttpClient> SignedInWithCookie(string account)
        {
            var client = _api.CreateClient();   // keeps the cookie the sign-in sets
            var response = await client.PostAsync("/api/Login", ApiFactory.Json(new { cuentaUsuario = account, contrasena = TestUsers.Password }));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            return client;
        }

        [TestCase("/OrderReport?value=1-1")]
        [TestCase("/OrderReport1?value=1-1")]
        [TestCase("/OrderReportF?value=1-1")]
        public async Task TheReports_OpenWithTheCookieOfTheSignInAlone(string url)
        {
            var client = await SignedInWithCookie("admin");

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, url);
            StringAssert.Contains("Service Order No.1", await response.Content.ReadAsStringAsync());
        }

        [TestCase("/OrderReport?value=1-1")]
        [TestCase("/OrderReport1?value=1-1")]
        [TestCase("/OrderReportF?value=1-1")]
        public async Task TheReports_AreRefusedWithoutASession(string url)
        {
            var response = await _api.CreateClient().GetAsync(url);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, url);
        }

        [TestCase("/OrderReport?value=1-2")]
        [TestCase("/OrderReport1?value=1-2")]
        [TestCase("/OrderReportF?value=1-2")]
        public async Task TheReports_OfAnotherCompany_AreForbidden(string url)
        {
            var client = await SignedInWithCookie("admin");

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, url);
        }

        [Test]
        public async Task TheCookie_IsNotAcceptedByTheApi()
        {
            var client = await SignedInWithCookie("admin");

            var response = await client.GetAsync("/api/User/2/unique");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, "the cookie only opens the printable reports");
        }

        [Test]
        public async Task AfterLogout_TheReportsAreRefusedAgain()
        {
            var client = await SignedInWithCookie("admin");
            await client.PostAsync("/api/Login/Logout", null);

            var response = await client.GetAsync("/OrderReport?value=1-1");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
