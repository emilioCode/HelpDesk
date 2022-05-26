using HelpDesk.Models;
using HelpDesk.Models.classes;
using HelpDesk.Models.Dictionaries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly HelpDeskDBContext context;

        public EventController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }

        // POST: api/Event
        [HttpPost]
        public async Task<JsonResult> Post([FromBody] Usuario usuario)
        {
            ObjectResponse res;
            Usuario user = await context.Usuario.FindAsync(usuario.Id);

            try
            {
                List<Event> events = new List<Event>();
                var resquests = context.Solicitud.Where(rq => rq.IdEmpresa == user.IdEmpresa);
                if (Levels.AccessLevel[user.Acceso] < Levels.AccessLevel["MODERADOR"]) resquests = resquests.Where(rq => rq.IdUsuario == user.Id);
                resquests.ToList().ForEach(rq =>
                 {
                     int HH = 0;
                     int mm = 0;
                     int ss = 0;

                     HH = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Hours: 0;
                     mm = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Minutes : 0;
                     ss = rq.HoraInicio.HasValue ? rq.HoraInicio.Value.Seconds : 0;

                     DateTime? fmtStart = rq.FechaInicio.HasValue ? new DateTime(rq.FechaInicio.Value.Year, rq.FechaInicio.Value.Month, rq.FechaInicio.Value.Day, HH, mm, ss): rq.FechaInicio;

                     HH = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Hours : 0;
                     mm = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Minutes : 0;
                     ss = rq.HoraTermino.HasValue ? rq.HoraTermino.Value.Seconds : 0;

                     DateTime? fmtEnd = rq.FechaTermino.HasValue ? new DateTime(rq.FechaTermino.Value.Year, rq.FechaTermino.Value.Month, rq.FechaTermino.Value.Day, HH, mm, ss): rq.FechaTermino;

                     events.Add(new Event
                     {
                         id = rq.Id,    
                         title = $"{ rq.TipoSolicitud } No.{ rq.NoSecuencia } | Estado: { rq.Estado }",
                         user =  context.Usuario.Find(rq.IdUsuario),
                         ticket = rq,
                         start = fmtStart,
                         end = fmtEnd
                     });
                 });
                res = new ObjectResponse
                {
                    data = events
                };
            }
            catch (Exception e)
            {
                res = new ObjectResponse
                {
                    code = "0",
                    title = "Error",
                    icon = "error",
                    message = e.Message,
                    data = null
                };
            }
            return new JsonResult(res);
        }
    }
}
