using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ITraceService
    {
        IEnumerable<CustomTrace> GetTracesById(int ticketId, int businessId);
        Task<Seguimiento> AddTrace(Seguimiento trace);
        Task<Seguimiento> UpdateTrace(Seguimiento trace);
    }
}
