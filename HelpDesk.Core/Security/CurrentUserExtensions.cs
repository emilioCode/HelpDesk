using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;

namespace HelpDesk.Core.Security
{
    // Rules shared by the services. They only apply when there is a signed-in user; without a token the
    // services keep their previous behavior, which is what the transition switch (Auth:Enforce) relies on.
    public static class CurrentUserExtensions
    {
        // The id sent by the client is only used when there is no token.
        public static int ResolveUserId(this ICurrentUser user, int requestedId)
        {
            return user.IsAuthenticated ? user.Id : requestedId;
        }

        public static void RequireLevel(this ICurrentUser user, int minimumLevel)
        {
            if (user.IsAuthenticated && user.Level < minimumLevel)
            {
                throw new AccessDeniedException();
            }
        }

        public static void RequireCompany(this ICurrentUser user, int companyId)
        {
            if (user.IsAuthenticated && user.CompanyId != companyId)
            {
                throw new AccessDeniedException();
            }
        }
    }
}
