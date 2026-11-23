using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Exceptions;
using HelpDesk.Core.Interfaces;
using HelpDesk.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class TraceService : ITraceService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        public TraceService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public IEnumerable<CustomTrace> GetTracesById(int ticketId, int businessId)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, ticketId);
            if (_currentUser.IsAuthenticated) businessId = _currentUser.CompanyId;
            var tickets = _unitOfWork.TicketRepository.GetAll().ToList();
            var traces = _unitOfWork.TraceRepository.GetAll().ToList();
            var users = _unitOfWork.UserRepository.GetAll().ToList();
            var customTraces = from seguimiento in traces
                                join solicitud in tickets on seguimiento.IdSolicitud equals solicitud.Id
                                join usuario in users on seguimiento.IdUsuario equals usuario.Id
                                where solicitud.Id == ticketId && solicitud.IdEmpresa == businessId
                                select new CustomTrace
                                {
                                    Id = seguimiento.Id,
                                    IdEmpresa = seguimiento.IdEmpresa,
                                    IdSolicitud = seguimiento.IdSolicitud,
                                    IdUsuario = seguimiento.IdUsuario,
                                    UserName = usuario.Nombre,
                                    UserImage = usuario.Image,
                                    Texto = seguimiento.Texto,
                                    Fecha = seguimiento.Fecha,
                                    Hora = seguimiento.Hora,
                                    Habilitado = seguimiento.Habilitado,
                                    Favorito = seguimiento.Favorito,
                                    Etiquetado = seguimiento.Etiquetado,
                                    VisibleAlCliente = seguimiento.VisibleAlCliente
                                };
            return customTraces;
        }

        public async Task<Seguimiento> AddTrace(Seguimiento trace)
        {
            _currentUser.EnsureTicketInCompany(_unitOfWork, trace.IdSolicitud);
            if (_currentUser.IsAuthenticated)
            {
                trace.IdEmpresa = _currentUser.CompanyId;
                trace.IdUsuario = _currentUser.Id;
            }
            trace.Fecha = DateTime.Now.Date;
            trace.Hora = DateTime.Now.TimeOfDay;
            trace.Favorito = false;
            trace.Etiquetado = false;
            trace.VisibleAlCliente = false;
            trace.Habilitado = true;
            await _unitOfWork.TraceRepository.Add(trace);
            await _unitOfWork.SaveChangeAsync();
            return trace;
        }

        public async Task<Seguimiento> UpdateTrace(Seguimiento trace)
        {
            if (_currentUser.IsAuthenticated)
            {
                var persisted = _unitOfWork.TraceRepository.GetAll().FirstOrDefault(x => x.Id == trace.Id);
                if (persisted == null) throw new AccessDeniedException();
                _currentUser.RequireCompany(persisted.IdEmpresa);
                trace.IdEmpresa = persisted.IdEmpresa;
                trace.IdSolicitud = persisted.IdSolicitud;
                trace.IdUsuario = persisted.IdUsuario;
                // Only SetVisibleToCustomerAsync changes it, so that marking an activity as favourite or solution does not undo it.
                trace.VisibleAlCliente = persisted.VisibleAlCliente;
            }
            _unitOfWork.TraceRepository.Update(trace);
            await _unitOfWork.SaveChangeAsync();
            return trace;
        }

        // The technician of the order, or a moderator or above, decides whether the customer sees an activity.
        public async Task<Seguimiento> SetVisibleToCustomerAsync(int traceId, bool visible)
        {
            var trace = _unitOfWork.TraceRepository.Query().FirstOrDefault(x => x.Id == traceId);
            if (trace == null) throw new AccessDeniedException();
            if (_currentUser.IsAuthenticated)
            {
                _currentUser.RequireCompany(trace.IdEmpresa);
                var ticket = _unitOfWork.TicketRepository.Query().FirstOrDefault(t => t.Id == trace.IdSolicitud);
                if (ticket == null || (_currentUser.Level < RoleLevel.Moderator && ticket.IdUsuario != _currentUser.Id)) throw new AccessDeniedException();
            }
            trace.VisibleAlCliente = visible;
            _unitOfWork.TraceRepository.Update(trace);
            await _unitOfWork.SaveChangeAsync();
            return trace;
        }

        public IEnumerable<Seguimiento> GetAllTraces()
        {
            return _unitOfWork.TraceRepository.GetAll();
        }
    }
}
