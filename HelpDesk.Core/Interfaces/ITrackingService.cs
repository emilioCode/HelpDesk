using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface ITrackingService
    {
        // The link of an order: the one that works, or a new one when there is none or it expired. For the staff of the order.
        Task<EnlaceSeguimiento> EnsureLinkAsync(int ticketId);

        // Stops every link of the order. The next call to EnsureLinkAsync makes a new one.
        Task RevokeAsync(int ticketId);

        // Starts the countdown of the link when the order is approved.
        Task OnApprovedAsync(int ticketId);

        // What a customer sees from the link, or null when the link does not exist, was revoked or expired.
        Task<TrackingViewDto> GetViewAsync(string token);

        // The e-mail with the link, or null when the customer has no address or the company cannot send mail.
        Task<EmailMessage> BuildEmailAsync(int ticketId, string url);

        string BuildUrl(string baseUrl, string token);
    }
}
