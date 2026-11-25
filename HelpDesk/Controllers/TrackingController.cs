using HelpDesk.Core.Constants;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _tracking;
        private readonly IMailService _mailService;

        public TrackingController(ITrackingService tracking, IMailService mailService)
        {
            _tracking = tracking;
            _mailService = mailService;
        }

        private string BaseUrl => $"{Request.Scheme}://{Request.Host}";

        // GET: api/Tracking/{token} — what a customer sees from the link. No sign in: the token is the key.
        // A link that does not exist, was revoked or expired all answer the same, so nobody can tell them apart.
        [AllowAnonymous]
        [EnableRateLimiting("tracking")]
        [HttpGet("{token}")]
        public async Task<IActionResult> Get(string token)
        {
            // The page is private to whoever has the link: not stored, not indexed, and the address is not passed on.
            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
            var view = await _tracking.GetViewAsync(token);
            if (view == null) return NotFound();
            return Ok(view);
        }

        // POST: api/Tracking/Link/5 — the link of an order, to give or print.
        [HttpPost("[action]/{ticketId}")]
        public async Task<IActionResult> Link(int ticketId)
        {
            var link = await _tracking.EnsureLinkAsync(ticketId);
            var dto = new TrackingLinkDto { Url = _tracking.BuildUrl(BaseUrl, link.Token), ExpiresAt = link.VenceEn };
            return Ok(ObjectResponse.Saved(Messages.Saved, dto));
        }

        // POST: api/Tracking/Email/5 — sends the link to the customer by e-mail.
        [HttpPost("[action]/{ticketId}")]
        public async Task<IActionResult> Email(int ticketId)
        {
            var link = await _tracking.EnsureLinkAsync(ticketId);
            var url = _tracking.BuildUrl(BaseUrl, link.Token);
            var message = await _tracking.BuildEmailAsync(ticketId, url);
            if (message == null)
            {
                return Ok(new ObjectResponse { code = "5", title = "Sin correo", icon = "warning", message = "El cliente no tiene correo o la empresa no tiene configurado el envío." });
            }
            _mailService.Send(message.Host, message.Port, System.Net.Mail.SmtpDeliveryMethod.Network, false, true, message.EmisorEmail, message.PasswordEmail, message.ReceptorEmail, message.Subject, message.Body, true);
            return Ok(ObjectResponse.Saved(Messages.Saved, new TrackingLinkDto { Url = url, ExpiresAt = link.VenceEn }));
        }

        // POST: api/Tracking/Revoke/5 — the old links stop working.
        [HttpPost("[action]/{ticketId}")]
        public async Task<IActionResult> Revoke(int ticketId)
        {
            await _tracking.RevokeAsync(ticketId);
            return Ok(ObjectResponse.Saved(Messages.Updated, null));
        }
    }
}
