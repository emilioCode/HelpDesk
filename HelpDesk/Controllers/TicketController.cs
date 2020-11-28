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
            {          
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
                Usuario usuario = context.Usuario.Find(req.IdUsuario);

                var subject = $"HelpDesk - Ticket No.{req.NoSecuencia}";
                var body = $@"<p>Saludos,</p>
                                <p>Usted tiene el ticket de {req.NoSecuencia} - <b>No.{req.NoSecuencia}</b></p>

                                <p>Este correo se genera de forma automática, favor no responder.</p>                            
                                ";

                var resp = MailClient.Send(empresa.Host, Convert.ToInt32(empresa.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                true, empresa.Correo, empresa.Contrasena,usuario.Correo , subject, body, true);

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
        [HttpPut("{toDo}")]
        public JsonResult Put(string toDo, [FromBody] Solicitud req)
        {
            ObjectResponse res;
            try
            {
                if (req.Titulo == null || req.Titulo == "" || req.IdEmpresa == null || req.IdEmpresa <= 0
                    || req.IdUsuario == null || req.IdUsuario <= 0 || req.IdCliente == null || req.IdCliente <= 0
                    || req.TipoSolicitud == null || req.TipoSolicitud == "" || req.TipoServicio == null || req.TipoServicio == "")
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "missing some field to complete",
                        data = null
                    };
                    return new JsonResult(res);
                }
                Solicitud ticket = context.Solicitud.Find(req.Id);
                if (toDo is "ESTADO")
                {
                    DateTime today = DateTime.Now.Date;
                    TimeSpan time = DateTime.Now.TimeOfDay;
                    ticket.Estado = req.Estado;
                    switch (req.Estado.ToUpper())
                    {
                        case "ABIERTO":
                            ticket.FechaTermino = null;
                            ticket.HoraTermino = null;
                            ticket.AprobadoPor = null;
                            break;
                        case "EN PROCESO":
                            ticket.FechaInicio = req.FechaInicio ==null?today: req.FechaInicio;
                            ticket.HoraInicio = req.FechaInicio==null? time: req.HoraInicio;
                            ticket.FechaTermino = null;
                            ticket.HoraTermino = null;
                            ticket.AprobadoPor = null;
                            break;
                        case "COMPLETADO":
                            if(ticket.FechaInicio is null) ticket.FechaInicio = today;
                            if (ticket.HoraInicio is null) ticket.HoraInicio = time;
                            ticket.FechaTermino = today;
                            ticket.HoraTermino = time;
                            break;
                        default:
                            break;
                    }
                    
                }
                else if(toDo is "USUARIO")
                {
                    ticket.IdUsuario = req.IdUsuario;

                    var subject = $"HelpDesk - Ticket No.{req.NoSecuencia}";
                    var body = $@"<p>Saludos,</p>
                                <p>El ticket de {req.NoSecuencia} - <b>No.{req.NoSecuencia}</b> se encuentra en su bandeja.</p>

                                <p>Este correo se genera de forma automática, favor no responder.</p>                            
                                ";
                    Empresa empresa = context.Empresa.Find(req.IdEmpresa);
                    Usuario usuario = context.Usuario.Find(req.IdUsuario);

                    var resp = MailClient.Send(empresa.Host, Convert.ToInt32(empresa.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                    true, empresa.Correo, empresa.Contrasena, usuario.Correo, subject, body, true);
                }
                else
                {
                    ticket.FechaInicio = req.FechaInicio;
                    ticket.HoraInicio = req.HoraInicio;
                    ticket.FechaTermino = req.FechaTermino;
                    ticket.HoraTermino = req.HoraTermino;
                    ticket.AprobadoPor = req.AprobadoPor;
                }
                context.Entry(ticket).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                if (toDo != "NOCHANGEPLEASE") context.SaveChanges();

                var ticket_res = context.Solicitud.Find(req.Id);

                
                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "has been updated successfully",
                    data = ticket_res
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

        // DELETE: api/ApiWithActions/5
        //[HttpDelete/*("{id}")*/]
        //public ObjectResponse Delete(/*int id*/)
        //{

        //    var res= MailClient.Send("smtp-mail.outlook.com", 587, System.Net.Mail.SmtpDeliveryMethod.Network, false,
        //        true, "emilio_mem@hotmail.com", "forever1234", "lsaulcastro@gmail.com", "MAIL TO TEST", "<h2 style='color:red'>MAIL TO TEST</h2>", true);
        //     //res = MailClient.Send("smtp-mail.outlook.com", 587, System.Net.Mail.SmtpDeliveryMethod.Network, false,

        //     //   true, "emilio_mem@hotmail.com", "forever1234", "Albertparedesdo @gmail.com", "MAIL TO TEST", "<h2 style='color:red'>MAIL TO TEST</h2>", true);

        //    return res;
        //}
        [HttpGet("[action]/{idUser}")]
        public async Task<JsonResult> numbersOfTickets(int idUser)
        {

            ObjectResponse response;
            try
            {
                Usuario usuario = await context.Usuario.FindAsync(idUser);
                var tickets = context.Solicitud.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario == usuario.Id).ToList();
                var abiertos = tickets.Where(e=>e.Estado.ToUpper() =="ABIERTO").Count();
                var enproceso = tickets.Where(e => e.Estado.ToUpper() != "ABIERTO" || e.Estado.ToUpper() != "EN PROCESO" || (e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null)).Count();
                var completados = tickets.Where(e => e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).Count();
                
                object data =  new
                {
                    abiertos ,
                    enproceso,
                    completados
                };

                response = new ObjectResponse
                {
                    code = "1",
                    data   = data
                };
            }
            catch (Exception ex)
            {
                response = new ObjectResponse
                {
                    title = "error",
                    code = "0",
                    message = ex.Message,
                    data = null
                };
            }
            return new JsonResult(response);
        }
    }
}
