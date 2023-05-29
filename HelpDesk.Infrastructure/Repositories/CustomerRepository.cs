using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Infrastructure.Repositories
{
    public class CustomerRepository : BaseRepository<Cliente>, ICustomerRepository
    {
        public CustomerRepository(HelpDeskDBContext context) : base(context)
        {
        }

        public IEnumerable<Cliente> GetByBusinessId(int BusinessId, bool isActive)
        {
            var customers = GetAll().Where(x => x.IdEmpresa == BusinessId);
            if (isActive) customers.Where(x => x.Habilitado == isActive);
            return customers;
        }

    }
}
