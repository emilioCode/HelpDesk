using HelpDesk.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IUserRepository : IRepository<Usuario>
    {
        IEnumerable<Usuario> GetByBusinessId(int BusinessId);
    }
}
