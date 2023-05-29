using HelpDesk.Core.Entities;
using System.Collections.Generic;

namespace HelpDesk.Core.Interfaces
{
    public interface IDeviceRepository : IRepository<Equipo>
    {
        IEnumerable<Equipo> GetDevicesByTicketId(int ticketId);
    }
}
