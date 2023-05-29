using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Dictionaries;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Event>> GetEvent(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
            var users = _unitOfWork.UserRepository.GetAll().ToList();
            var events = new List<Event>();
            var resquests = _unitOfWork.TicketRepository.GetAll().ToList().Where(rq => rq.IdEmpresa == user.IdEmpresa);
            if (Levels.AccessLevel[user.Acceso] < Levels.AccessLevel["MODERADOR"]) resquests = resquests.Where(rq => rq.IdUsuario == user.Id);
            resquests.ToList().ForEach(rq =>
            {
                int HH = 0;
                int mm = 0;
                int ss = 0;

                HH = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Hours : 0;
                mm = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Minutes : 0;
                ss = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Seconds : 0;

                DateTime? fmtStart = rq.FechaInicio.HasValue ? new DateTime(rq.FechaInicio.Value.Year, rq.FechaInicio.Value.Month, rq.FechaInicio.Value.Day, HH, mm, ss) : rq.FechaInicio;

                HH = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Hours : 0;
                mm = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Minutes : 0;
                ss = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Seconds : 0;

                DateTime? fmtEnd = rq.FechaTermino.HasValue ? new DateTime(rq.FechaTermino.Value.Year, rq.FechaTermino.Value.Month, rq.FechaTermino.Value.Day, HH, mm, ss) : rq.FechaTermino;

                events.Add(new Event
                {
                    id = rq.Id,
                    title = $"{rq.TipoSolicitud} No.{rq.NoSecuencia} | Estado: {rq.Estado}",
                    user = users.Where(x => x.Id == rq.IdUsuario).SingleOrDefault(),
                    ticket = rq,
                    start = fmtStart,
                    end = fmtEnd
                });
            });
            return events;
        }
    }
}
