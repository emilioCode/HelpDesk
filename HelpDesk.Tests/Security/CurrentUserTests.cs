using HelpDesk.Security;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System.Security.Claims;

namespace HelpDesk.Tests.Security
{
    public class CurrentUserTests
    {
        private static CurrentUser For(ClaimsPrincipal principal)
        {
            var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } };
            return new CurrentUser(accessor);
        }

        private static ClaimsPrincipal SignedIn(string id, string role, string company)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtTokenService.CompanyClaim, company)
            }, "test");
            return new ClaimsPrincipal(identity);
        }

        [Test]
        public void ReadsTheClaimsOfTheSignedInUser()
        {
            var user = For(SignedIn("5", "MODERADOR", "2"));

            Assert.IsTrue(user.IsAuthenticated);
            Assert.AreEqual(5, user.Id);
            Assert.AreEqual(2, user.CompanyId);
            Assert.AreEqual("MODERADOR", user.Role);
            Assert.AreEqual(2, user.Level);
        }

        [TestCase("ROOT", 4)]
        [TestCase("ADMINISTRADOR", 3)]
        [TestCase("MODERADOR", 2)]
        [TestCase("TECNICO", 1)]
        [TestCase("OTHER", 0)]
        public void Level_FollowsTheRole(string role, int level)
        {
            Assert.AreEqual(level, For(SignedIn("1", role, "1")).Level);
        }

        [Test]
        public void AnonymousRequest_IsNotAuthenticated()
        {
            var user = For(new ClaimsPrincipal(new ClaimsIdentity()));

            Assert.IsFalse(user.IsAuthenticated);
            Assert.AreEqual(0, user.Id);
            Assert.AreEqual(0, user.CompanyId);
        }

        [Test]
        public void WithoutAnHttpContext_IsNotAuthenticated()
        {
            var user = new CurrentUser(new HttpContextAccessor());
            Assert.IsFalse(user.IsAuthenticated);
        }
    }
}
