using HelpDesk.Core.CustomEntities;
using HelpDesk.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace HelpDesk.Tests.Security
{
    public class JwtTokenServiceTests
    {
        private const string Key = "a-test-jwt-key-with-more-than-32-characters";

        private static JwtTokenService CreateService(int minutes = 480)
        {
            return new JwtTokenService(Options.Create(new AuthOptions { Key = Key, ExpiresMinutes = minutes }));
        }

        private static TokenValidationParameters Parameters(string key = Key)
        {
            return new TokenValidationParameters
            {
                ValidIssuer = "HelpDesk",
                ValidAudience = "HelpDesk",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.Zero
            };
        }

        [Test]
        public void Create_IncludesTheUserCompanyAndRole()
        {
            var identity = new UserIdentity { Id = 7, UserName = "ana", Acceso = "ADMINISTRADOR", IdEmpresa = 3 };

            var (token, expiresAt) = CreateService().Create(identity);
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, Parameters(), out _);

            Assert.AreEqual("7", principal.FindFirst(ClaimTypes.NameIdentifier).Value);
            Assert.AreEqual("ana", principal.FindFirst(ClaimTypes.Name).Value);
            Assert.AreEqual("ADMINISTRADOR", principal.FindFirst(ClaimTypes.Role).Value);
            Assert.AreEqual("3", principal.FindFirst(JwtTokenService.CompanyClaim).Value);
            Assert.Greater(expiresAt, DateTime.UtcNow.AddMinutes(479));
        }

        [Test]
        public void Create_NeverIncludesPasswordsOrImages()
        {
            var identity = new UserIdentity { Id = 1, Image = new byte[] { 1, 2, 3 }, Logo = new byte[] { 4, 5 } };

            var token = new JwtSecurityTokenHandler().ReadJwtToken(CreateService().Create(identity).Token);

            var allowed = new[] { ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Role, JwtTokenService.CompanyClaim, "jti", "nbf", "exp", "iat", "iss", "aud" };
            CollectionAssert.IsSubsetOf(token.Claims.Select(c => c.Type).Distinct(), allowed);
        }

        [Test]
        public void Token_IsRejectedWithAnotherKey()
        {
            var token = CreateService().Create(new UserIdentity { Id = 1 }).Token;
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(token, Parameters("a-different-key-with-more-than-32-characters"), out _));
        }

        [Test]
        public void Token_IsRejectedWhenItHasBeenAltered()
        {
            var token = CreateService().Create(new UserIdentity { Id = 1, Acceso = "TECNICO" }).Token;
            var parts = token.Split('.');
            var payload = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parts[1])).Replace("TECNICO", "ROOT");
            var forged = parts[0] + "." + Base64UrlEncoder.Encode(payload) + "." + parts[2];

            Assert.That(() => new JwtSecurityTokenHandler().ValidateToken(forged, Parameters(), out _),
                Throws.InstanceOf<SecurityTokenInvalidSignatureException>());
        }

        [Test]
        public void Token_ExpiresAfterTheConfiguredTime()
        {
            var (token, expiresAt) = CreateService(minutes: 1).Create(new UserIdentity { Id = 1 });
            Assert.LessOrEqual(expiresAt, DateTime.UtcNow.AddMinutes(1).AddSeconds(2));

            var expired = new TokenValidationParameters
            {
                ValidIssuer = "HelpDesk", ValidAudience = "HelpDesk",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = (before, expires, t, p) => DateTime.UtcNow.AddMinutes(2) < expires
            };
            Assert.Throws<SecurityTokenInvalidLifetimeException>(() =>
                new JwtSecurityTokenHandler().ValidateToken(token, expired, out _));
        }
    }
}
