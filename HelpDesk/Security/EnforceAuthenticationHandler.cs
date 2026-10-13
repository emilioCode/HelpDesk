using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HelpDesk.Security
{
    // While Auth:Enforce is off, anonymous requests satisfy the "signed-in user required" rule of the
    // fallback policy, so the existing front end keeps working until it sends the token.
    public class EnforceAuthenticationHandler : AuthorizationHandler<DenyAnonymousAuthorizationRequirement>
    {
        private readonly AuthOptions _options;

        public EnforceAuthenticationHandler(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DenyAnonymousAuthorizationRequirement requirement)
        {
            if (!_options.Enforce)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
