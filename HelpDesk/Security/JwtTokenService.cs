using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HelpDesk.Security
{
    public class JwtTokenService : ITokenService
    {
        public const string CompanyClaim = "cid";

        private readonly AuthOptions _options;

        public JwtTokenService(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        public (string Token, DateTime ExpiresAt) Create(UserIdentity user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.ExpiresMinutes);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Acceso ?? string.Empty),
                new Claim(CompanyClaim, user.IdEmpresa.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, now, expires, credentials);
            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
