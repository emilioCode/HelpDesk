using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<SolicitudT>> GetTicketsByIdAndCondition(int id, string option);
        Task<(Solicitud, EmailMessage)> CreateTicket(Solicitud ticket);
        IEnumerable<SummaryInformation> GetSummaryInformation(Solicitud ticket);
        Task<Metric> GetMetricsByUserId(int userId);
        Task<(Solicitud, EmailMessage)> UpdateTicket(Solicitud ticket, string action);
        IEnumerable<Solicitud> GetTicketBySecuencialNumber(string secuencialNumber);
    }
}
