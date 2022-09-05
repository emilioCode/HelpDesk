using HelpDesk.Core.Entities;
using System.Collections.Generic;

namespace HelpDesk.Core.Interfaces
{
    public interface ITicketRepository : IRepository<Solicitud>
    {
        IEnumerable<Solicitud> GetTicketsByUserId(int userId);
    }
}
