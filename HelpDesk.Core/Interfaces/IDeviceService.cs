using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IDeviceService
    {
        IEnumerable<Equipo> GetDevicesByTicketId(int ticketId);
        Task<Equipo> AddDevice(Equipo device);
        Task<List<Equipo>> AddRangeDevice(List<Equipo> devices);
        Task<bool> DeleteDevide(int deviceId);
        Task<Equipo> ModifyDevice(Equipo device);
    }
}
