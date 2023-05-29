using HelpDesk.Core.Entities;
using System.Collections.Generic;

namespace HelpDesk.Core.Interfaces
{
    public interface ICustomerRepository : IRepository<Cliente>
    {
        IEnumerable<Cliente> GetByBusinessId(int BusinessId, bool isActive);
    }
}
