using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class TraceService : ITraceService
    {
        private IUnitOfWork _unitOfWork;    
        public TraceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<CustomTrace> GetTracesById(int ticketId, int businessId)
        {
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
                                    Etiquetado = seguimiento.Etiquetado
                                };
            return customTraces;
        }

        public async Task<Seguimiento> AddTrace(Seguimiento trace)
        {
            trace.Fecha = DateTime.Now.Date;
            trace.Hora = DateTime.Now.TimeOfDay;
            trace.Favorito = false;
            trace.Etiquetado = false;
            trace.Habilitado = true;
            await _unitOfWork.TraceRepository.Add(trace);
            await _unitOfWork.SaveChangeAsync();
            return trace;
        }

        public async Task<Seguimiento> UpdateTrace(Seguimiento trace)
        {
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
