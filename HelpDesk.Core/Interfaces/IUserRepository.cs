using HelpDesk.Core.Entities;
using System.Collections.Generic;

namespace HelpDesk.Core.Interfaces
{
    public interface IUserRepository : IRepository<Usuario>
    {
        IEnumerable<Usuario> GetByBusinessId(int BusinessId);
    }
}
