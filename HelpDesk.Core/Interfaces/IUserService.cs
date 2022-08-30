using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IUserService
    {
        IEnumerable<Usuario> GetUsers();
        Task<Usuario> GetUserById(int id);
        Task<Usuario> InsertUSer(Usuario user);
        Task<bool> UpdateUSer(Usuario user, int userCreatorId);
        Task<bool> Delete(int id);
        Task<List<Usuario>> GetUsersByIdAndCondition(int id, string condition);
    }
}
