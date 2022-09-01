using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<Usuario>, IUserRepository
    {
        public UserRepository(HelpDeskDBContext context) : base(context)
        {

        }

        public  IEnumerable<Usuario> GetByBusinessId(int BusinessId)
        {
            return GetAll().Where(x => x.IdEmpresa == BusinessId);
        }
    }
}
