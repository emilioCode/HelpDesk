using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IBusinessService
    {
        Task<IEnumerable<Empresa>> GetBusinessesByUserAccess(int userId);
        Task<Empresa> CreateBusinness(int userId, Empresa business);
        Task<bool> UpdateBusiness(int userId, Empresa business);
    }
}
