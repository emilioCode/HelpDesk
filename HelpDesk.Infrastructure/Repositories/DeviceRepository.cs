using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Infrastructure.Repositories
{
    public class DeviceRepository : BaseRepository<Equipo>, IDeviceRepository
    {
        public DeviceRepository(HelpDeskDBContext context) : base(context)
        {
            
        }

        public IEnumerable<Equipo> GetDevicesByTicketId(int ticketId)
        {
            var devices = GetAll().Where(x => x.IdSolicitud == ticketId);
            return devices;
        }
    }
}
