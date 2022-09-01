using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Cliente>> GetCustomersByIdAndCondition(int id, string option = null);
        Task<Cliente> CreateCustomer(Cliente customer);
        Task<bool> UpdateCustomer(Cliente customer, int userCreatorId);
    }
}
