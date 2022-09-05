using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Infrastructure.Repositories
{
    public class TicketRepository : BaseRepository<Solicitud>, ITicketRepository
    {
        public TicketRepository(HelpDeskDBContext context) : base(context)
        {
        }

        public IEnumerable<Solicitud> GetTicketsByUserId(int userId)
        {
            var tickets = GetAll().Where(x => x.IdUsuario == userId).OrderByDescending(x => x.Id);
            return tickets;
        }
    }
}
