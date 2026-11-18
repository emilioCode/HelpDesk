using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    // A browser that keeps an old copy of the app keeps calling the API the old way. index.html must always be checked
    // with the server, and only files named after their content may be kept for a long time.
    public class StaticFilesCacheTests
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

        [TestCase("/")]
        [TestCase("/ticket")]
        [TestCase("/index.html")]
        public async Task TheStartPage_IsAlwaysCheckedWithTheServer(string url)
        {
            var response = await _api.CreateClient().GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, url);
            Assert.AreEqual("no-cache", response.Headers.CacheControl.ToString(), url);
        }

        [TestCase("/main.0123456789abcdef0123.js")]
        [TestCase("/main-es2015.0123456789abcdef0123.js")]
        [TestCase("/main-es5.0123456789abcdef0123.js")]
        [TestCase("/main.0123456789abcdef.js")]
        [TestCase("/main.01234567.js")]
        [TestCase("/main-4ZCH2Y4H.js")]
        [TestCase("/styles-PMOLJ6K4.css")]
        public async Task ABundleNamedAfterItsContent_CanBeKeptForAYear(string url)
        {
            var response = await _api.CreateClient().GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            StringAssert.Contains("max-age=31536000", response.Headers.CacheControl.ToString());
            StringAssert.Contains("immutable", response.Headers.CacheControl.ToString());
        }

        [TestCase("/main.js")]
        [TestCase("/chunk-main.js")]
        public async Task AFileWithoutAContentHash_IsCheckedWithTheServer(string url)
        {
            var response = await _api.CreateClient().GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("no-cache", response.Headers.CacheControl.ToString());
        }
    }
}
