using HelpDesk.Core.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    // Without an ORDER BY the database may return the same rows in a different order from one call to the next, and the
    // calendar showed equal events in a different place each time. The lists come back by Id.
    public class ListOrderTests
    {
        private ApiFactory _api;

        [OneTimeSetUp]
        public void SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
            // Saved out of order on purpose: the order of the database is the order they were written in.
            _api.Query(db =>
            {
                var today = DateTime.Now.Date;
                db.Solicitudes.AddRange(
                    ApiFactory.Ticket(53, "53", 1, 4, 1, today),
                    ApiFactory.Ticket(51, "51", 1, 4, 1, today),
                    ApiFactory.Ticket(52, "52", 1, 4, 1, today));
                db.SaveChanges();
                return 0;
            });
        }

        [OneTimeTearDown]
        public void TearDown() => _api.Dispose();

        [Test]
        public async Task TheEventsOfTheCalendar_ComeInOrderOfId()
        {
            var admin = await _api.ClientFor("admin");

            var response = await admin.PostAsync("/api/Event", ApiFactory.Json(new { id = 2 }));
            var ids = ((JArray)JObject.Parse(await response.Content.ReadAsStringAsync())["data"]).Select(e => (int)e["id"]).ToList();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.IsOrdered(ids);
            CollectionAssert.IsSupersetOf(ids, new[] { 51, 52, 53 });
        }
    }
}
