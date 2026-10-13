using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using System.Linq;

namespace HelpDesk.Core.Security
{
    public static class TicketAccess
    {
        // Devices, parts and activities hang from a ticket: the ticket must belong to the signed-in user's company.
        public static void EnsureTicketInCompany(this ICurrentUser user, IUnitOfWork unitOfWork, int ticketId)
        {
            if (!user.IsAuthenticated) return;
            var ticket = unitOfWork.TicketRepository.GetAll().FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null || ticket.IdEmpresa != user.CompanyId)
            {
                throw new AccessDeniedException();
            }
        }
    }
}
