using HelpDesk.Core.Constants;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly ISecretProtector _secretProtector;
        private readonly TrackingOptions _options;

        public TrackingService(IUnitOfWork unitOfWork, ICurrentUser currentUser, ISecretProtector secretProtector, TrackingOptions options)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _secretProtector = secretProtector;
            _options = options;
        }

        // The staff may manage the link of the orders of their company: moderators and above for any, a technician for their own.
        private Solicitud RequireAccess(int ticketId)
        {
            var ticket = _unitOfWork.TicketRepository.Query().FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null) throw new AccessDeniedException();
            if (_currentUser.IsAuthenticated)
            {
                _currentUser.RequireCompany(ticket.IdEmpresa);
                if (_currentUser.Level < RoleLevel.Moderator && ticket.IdUsuario != _currentUser.Id) throw new AccessDeniedException();
            }
            return ticket;
        }

        private static bool IsApproved(Solicitud ticket)
        {
            return ticket.AprobadoPor != null && string.Equals(ticket.Estado, TicketStatus.Completed, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsActive(EnlaceSeguimiento link)
        {
            return link.RevocadoEn == null && link.Habilitado != false;
        }

        // An approved order's link lasts a while after the approval; before that it has no end.
        private static bool IsUsable(EnlaceSeguimiento link, Solicitud ticket, DateTime now)
        {
            return IsActive(link) && (link.VenceEn == null || link.VenceEn > now || !IsApproved(ticket));
        }

        private static string NewToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public async Task<EnlaceSeguimiento> EnsureLinkAsync(int ticketId)
        {
            var ticket = RequireAccess(ticketId);
            var now = DateTime.UtcNow;
            var current = _unitOfWork.TrackingRepository.Query()
                .Where(l => l.IdSolicitud == ticketId && l.RevocadoEn == null && l.Habilitado != false)
                .OrderByDescending(l => l.Id).FirstOrDefault();
            if (current != null && IsUsable(current, ticket, now)) return current;

            if (current != null)
            {
                current.RevocadoEn = now;
                _unitOfWork.TrackingRepository.Update(current);
            }
            var link = new EnlaceSeguimiento
            {
                IdEmpresa = ticket.IdEmpresa,
                IdSolicitud = ticketId,
                Token = NewToken(),
                CreadoEn = now,
                VenceEn = IsApproved(ticket) ? now.AddDays(_options.DaysAfterApproval) : (DateTime?)null,
                Habilitado = true
            };
            await _unitOfWork.TrackingRepository.Add(link);
            await _unitOfWork.SaveChangeAsync();
            return link;
        }

        public async Task RevokeAsync(int ticketId)
        {
            RequireAccess(ticketId);
            var now = DateTime.UtcNow;
            foreach (var link in _unitOfWork.TrackingRepository.Query().Where(l => l.IdSolicitud == ticketId && l.RevocadoEn == null).ToList())
            {
                link.RevocadoEn = now;
                _unitOfWork.TrackingRepository.Update(link);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task OnApprovedAsync(int ticketId)
        {
            var ends = DateTime.UtcNow.AddDays(_options.DaysAfterApproval);
            foreach (var link in _unitOfWork.TrackingRepository.Query().Where(l => l.IdSolicitud == ticketId && l.RevocadoEn == null && l.VenceEn == null).ToList())
            {
                link.VenceEn = ends;
                _unitOfWork.TrackingRepository.Update(link);
            }
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<TrackingViewDto> GetViewAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length > 64 || !token.All(c => char.IsAsciiLetterOrDigit(c) || c == '-' || c == '_')) return null;
            var link = _unitOfWork.TrackingRepository.Query().FirstOrDefault(l => l.Token == token);
            if (link == null) return null;
            var ticket = _unitOfWork.TicketRepository.Query().FirstOrDefault(t => t.Id == link.IdSolicitud && t.Habilitado != false);
            if (ticket == null || !IsUsable(link, ticket, DateTime.UtcNow)) return null;

            var company = await _unitOfWork.BusinessRepository.GetById(ticket.IdEmpresa);
            var customer = await _unitOfWork.CustomerRepository.GetById(ticket.IdCliente);
            var traces = _unitOfWork.TraceRepository.Query()
                .Where(t => t.IdSolicitud == ticket.Id && t.IdEmpresa == ticket.IdEmpresa && t.VisibleAlCliente == true && t.Habilitado == true)
                .OrderBy(t => t.Fecha).ThenBy(t => t.Hora).ToList();
            var userIds = traces.Select(t => t.IdUsuario).Distinct().ToList();
            var names = _unitOfWork.UserRepository.Query().Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.Nombre }).ToList()
                .ToDictionary(u => u.Id, u => u.Nombre);

            var (status, step, progress) = StatusOf(ticket);
            var view = new TrackingViewDto
            {
                OrderNumber = ticket.NoSecuencia,
                Company = company?.RazonSocial,
                CompanyPhone = company?.Telefono,
                CompanyEmail = company?.Correo,
                Customer = customer?.Nombre,
                ServiceType = ticket.TipoSolicitud,
                Status = status,
                Step = step,
                Progress = progress,
                UpdatedAt = ticket.FechaCreacion
            };
            foreach (var t in traces)
            {
                var when = t.Fecha.Add(t.Hora);
                if (when > view.UpdatedAt) view.UpdatedAt = when;
                names.TryGetValue(t.IdUsuario, out var name);
                view.Activities.Add(new TrackingActivityDto { Who = FirstName(name), When = when, Text = t.Texto, IsSolution = t.Etiquetado == true });
            }
            return view;
        }

        private static (string, int, int) StatusOf(Solicitud ticket)
        {
            if (IsApproved(ticket)) return ("approved", 4, 100);
            if (string.Equals(ticket.Estado, TicketStatus.Completed, StringComparison.OrdinalIgnoreCase)) return ("completed", 3, 90);
            if (string.Equals(ticket.Estado, TicketStatus.InProgress, StringComparison.OrdinalIgnoreCase)) return ("inProgress", 2, 60);
            return ("open", 1, 25);
        }

        private static string FirstName(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().Split(' ')[0];
        }

        public async Task<EmailMessage> BuildEmailAsync(int ticketId, string url)
        {
            var ticket = RequireAccess(ticketId);
            var business = await _unitOfWork.BusinessRepository.GetById(ticket.IdEmpresa);
            var customer = await _unitOfWork.CustomerRepository.GetById(ticket.IdCliente);
            if (customer == null || string.IsNullOrWhiteSpace(customer.Correo) || business == null
                || string.IsNullOrWhiteSpace(business.Host) || business.Port == null || string.IsNullOrWhiteSpace(business.Correo)) return null;

            var company = WebUtility.HtmlEncode(business.RazonSocial ?? string.Empty);
            var name = WebUtility.HtmlEncode(customer.Nombre ?? string.Empty);
            var number = WebUtility.HtmlEncode(ticket.NoSecuencia ?? string.Empty);
            var link = WebUtility.HtmlEncode(url);
            var body = $@"<div style=""font-family:Arial,sans-serif;max-width:560px"">
<h2 style=""margin:0 0 12px"">{company}</h2>
<p>Hola {name},</p>
<p>Puede ver el estado de su orden de servicio <b>No. {number}</b> y lo que hemos hecho en cualquier momento, desde su teléfono o su computadora:</p>
<p><a href=""{link}"" style=""display:inline-block;padding:12px 20px;background:#2451D6;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:bold"">Ver mi orden</a></p>
<p style=""color:#5A6472;font-size:13px"">Si el botón no abre, copie esta dirección en su navegador:<br>{link}</p>
<p style=""color:#5A6472;font-size:13px"">Este enlace es personal: no lo comparta.</p>
</div>";
            return new EmailMessage
            {
                Subject = $"Siga su orden No. {ticket.NoSecuencia} - {business.RazonSocial}",
                Body = body,
                ReceptorEmail = customer.Correo,
                Host = business.Host,
                Port = business.Port.Value,
                EmisorEmail = business.Correo,
                PasswordEmail = _secretProtector.Unprotect(business.Contrasena)
            };
        }

        public string BuildUrl(string baseUrl, string token)
        {
            var root = !string.IsNullOrWhiteSpace(_options.PublicBaseUrl) ? _options.PublicBaseUrl : baseUrl;
            return root.TrimEnd('/') + "/t/" + token;
        }
    }
}
