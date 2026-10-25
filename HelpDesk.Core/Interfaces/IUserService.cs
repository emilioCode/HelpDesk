using HelpDesk.Core.CustomEntities;
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
        Task<IEnumerable<Usuario>> GetUsersByIdAndCondition(int id, string condition);
        Task<UserIdentity> LoginAsync(string account, string password);

        // The identity of a user who is still allowed to sign in, read again from the database; null otherwise.
        Task<UserIdentity> GetActiveIdentityAsync(int userId);
        Task<bool> ValidateUserAccount (int idEmpresa, string userAccount);
    }
}
