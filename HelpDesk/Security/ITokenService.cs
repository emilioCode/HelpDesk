using HelpDesk.Core.CustomEntities;
using System;

namespace HelpDesk.Security
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) Create(UserIdentity user);
    }
}
