using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models;
using HelpDesk.Models.classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public TicketController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        // GET: api/Ticket
        [HttpGet("{idUser}/{option}")]
        public JsonResult Get(int idUser, string option = "unique")
        {
            List<Solicitud> solicitudes = new List<Solicitud>();

            if (option.ToLower() == "unique")
            {
                var array = context.Solicitud.Where(s => s.IdUsuario == idUser).OrderByDescending(s=>s.Id);
                solicitudes.AddRange(array);
            }
            else if (option == "*" || option.ToLower() == "all")
            {                                            
                Usuario usuario = context.Usuario.Find(idUser);
                switch (usuario.Acceso)
                {
                    case "ROOT":
                    //break;
                    case "ADMINISTRADOR":
                        //break;
                    case "MODERADOR":
                        solicitudes.AddRange(context.Solicitud.Where(u => u.IdEmpresa == usuario.IdEmpresa).OrderByDescending(x => x.Id));
                    break;
                    case "TECNICO":
                    //break;
                    default:
                        break;
                }
            }
            return new JsonResult(solicitudes);
        }

        //// GET: api/Ticket/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Ticket
        [HttpPost]
        public JsonResult Post([FromBody] Solicitud req)
        {
            ObjectResponse res;
            try
            {            //Falta fecha creacion y Tipo de Servicio
                if (req.Titulo == null || req.Titulo == "" || req.IdEmpresa == null || req.IdEmpresa <= 0
                    || req.IdUsuario == null || req.IdUsuario <= 0 || req.IdCliente == null || req.IdCliente <= 0
                    || req.TipoSolicitud == null || req.TipoSolicitud == "" || req.TipoServicio == null || req.TipoServicio == "")
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "'Field(s) required!'\nCheck title, costumer, user, type of request or type of service",
                        data = null
                    };
                    return new JsonResult(res);
                }
                Empresa empresa = context.Empresa.Find(req.IdEmpresa);
                req.FechaCreacion = DateTime.Now.Date;
                req.Habilitado = true;
                req.NoSecuencia = empresa.Secuenciaticket == string.Empty ? "1" : empresa.Secuenciaticket;
                empresa.Secuenciaticket = (Convert.ToInt32(req.NoSecuencia)+1).ToString();
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.Entry(empresa).State = Microsoft.EntityFrameworkCore.EntityState.Modified;  
                context.SaveChanges();

                var data = context.Solicitud.Where(e => e.NoSecuencia == req.NoSecuencia).LastOrDefault();
                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "created successfully",
                    data = data
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

        // PUT: api/Ticket/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
