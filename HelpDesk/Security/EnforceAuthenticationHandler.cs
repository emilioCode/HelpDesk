using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HelpDesk.Security
{
    // Decides when the "signed-in user required" rule of the fallback policy applies.
    //  - While Auth:Enforce is off, anonymous requests are let through (transition mode).
    //  - Requests that reach no endpoint (the single page app's index.html and its deep links, which are served after
    //    the authorization step) are always let through, otherwise nobody could load the sign-in screen. They carry no data.
    public class EnforceAuthenticationHandler : AuthorizationHandler<DenyAnonymousAuthorizationRequirement>
    {
        private readonly AuthOptions _options;

        public EnforceAuthenticationHandler(IOptions<AuthOptions> options)
        {
            _options = options.Value;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DenyAnonymousAuthorizationRequirement requirement)
        {
            if (!_options.Enforce || ReachesNoEndpoint(context))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }

        private static bool ReachesNoEndpoint(AuthorizationHandlerContext context)
        {
            var endpoint = context.Resource is HttpContext http ? http.GetEndpoint() : context.Resource as Microsoft.AspNetCore.Http.Endpoint;
            return endpoint == null;
        }
    }
}
