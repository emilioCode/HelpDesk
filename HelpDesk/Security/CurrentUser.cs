using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HelpDesk.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal Principal => _accessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && Id > 0;

        public int Id => ReadInt(ClaimTypes.NameIdentifier);

        public int CompanyId => ReadInt(JwtTokenService.CompanyClaim);

        public string Role => Principal?.FindFirst(ClaimTypes.Role)?.Value;

        public int Level => RoleLevel.Of(Role);

        private int ReadInt(string claimType)
        {
            return int.TryParse(Principal?.FindFirst(claimType)?.Value, out var value) ? value : 0;
        }
    }
}
