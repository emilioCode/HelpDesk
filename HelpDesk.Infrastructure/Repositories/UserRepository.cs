using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDesk.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<Usuario>, IUserRepository
    {
        public UserRepository(HelpDeskDBContext context) : base(context)
        {

        }

        public  IEnumerable<Usuario> GetByBusinessId(int BusinessId)
        {
            return GetAll().Where(x => x.IdEmpresa == BusinessId).AsEnumerable();
        }
    }
}
