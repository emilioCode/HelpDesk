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
                if ( req.IdEmpresa == null || req.IdEmpresa <= 0|| req.IdCliente == null || req.IdCliente <= 0
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

                req.IdUsuario = req.IdUsuario < 1 ? 0 : req.IdUsuario;
                req.FechaCreacion = DateTime.Now.Date;
                req.Habilitado = true;
                req.NoSecuencia = empresa.Secuenciaticket == string.Empty ? "1" : empresa.Secuenciaticket;
                empresa.Secuenciaticket = (Convert.ToInt32(req.NoSecuencia)+1).ToString();
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.Entry(empresa).State = Microsoft.EntityFrameworkCore.EntityState.Modified;  
                context.SaveChanges();


                Cliente cliente = context.Cliente.Find(req.IdCliente);
                
                if(req.IdUsuario != 0 )
                {
                    Usuario usuario = context.Usuario.Where(u => u.IdEmpresa == req.IdEmpresa && u.Id == req.IdUsuario).SingleOrDefault();
                    var subject = $"HelpDesk Notification - Ticket No.{req.NoSecuencia}";

                    var discrepancia = req.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                    var body = $@"<div>
                                {empresa.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{usuario.Nombre}</i></b><br>
                                Fecha: <b><i>{req.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{req.NoSecuencia}</i></b><br>
                                Cliente: <b><i>{cliente.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{req.Descripcion}</i></b><br>
                            </div>";

                    var resp = MailClient.Send(empresa.Host, Convert.ToInt32(empresa.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                    true, empresa.Correo, empresa.Contrasena, usuario.Correo, subject, body, true);

                }

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
                if (req.IdEmpresa == null || req.IdEmpresa <= 0|| req.IdUsuario == null || req.IdUsuario <= 0 || req.IdCliente == null || req.IdCliente <= 0
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
                Empresa empresa = context.Empresa.Find(req.IdEmpresa);
                Usuario usuario = context.Usuario.Find(req.IdUsuario);
                Cliente cliente = context.Cliente.Find(req.IdCliente);

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

                    var subject = $"HelpDesk Notification - Ticket No.{req.NoSecuencia}";
                    var discrepancia = req.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                    var body = $@"<div>
                                {empresa.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{usuario.Nombre}</i></b><br>
                                Fecha: <b><i>{req.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{req.NoSecuencia}</i></b><br>
                                Cliente: <b><i>{cliente.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{req.Descripcion}</i></b><br>
                            </div>";

                    //Empresa empresa = context.Empresa.Find(req.IdEmpresa);
                    //Usuario usuario = context.Usuario.Find(req.IdUsuario);

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

        //DELETE: api/ApiWithActions/5
        //[HttpDelete/*("{id}")*/]
        //public ObjectResponse Delete(/*int id*/)
        //{
        //    var today = DateTime.Today.Date;
        //    var body = $@"<div>
        //                            RAMVAR Computadoras, SRL.<br>
        //                            Orden:<b><i>20200001</i></b><br>
        //                            Orden de Servicio a Domicilio<br> 
        //                            tecnico asignado:<b><i>tecnico #1</i></b><br>
        //                            Fecha:{today.ToString("dd/MM/yyyy")}<br>
        //                            Cliente:<b><i>Mix Viajes & Cruceros</i></b><br>
        //                            Falla: XXXXXX<br>
        //                        </div>";
        //    var res = MailClient.Send("smtp-mail.outlook.com", 587, System.Net.Mail.SmtpDeliveryMethod.Network, false,
        //        true, "emilio_mem@hotmail.com", "forever1234", "emilio_mem@hotmail.com,thekingemilio@gmail.com", "MAIL TO TEST", body, true);
        //    //res = MailClient.Send("smtp-mail.outlook.com", 587, System.Net.Mail.SmtpDeliveryMethod.Network, false,

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
                var enproceso = tickets.Where(e => e.Estado.ToUpper() != "ABIERTO" || e.Estado.ToUpper() == "EN PROCESO" || (e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null)).Count();
                var completados = tickets.Where(e => e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).Count();
                var costumers = context.Cliente.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.Habilitado == true).ToList().Count();
                var waitingToAttend = usuario.Acceso == "TECNICO"   ? 0 : context.Solicitud.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO").ToList().Count();

                object data = new
                {
                    abiertos,
                    enproceso,
                    completados,
                    costumers,
                    waitingToAttend
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

        [HttpPost("[action]")]
        public JsonResult GetJsonTicket([FromBody] Solicitud solicitud)
        {//parameters: idEmpresa, fechaInicio(from), fechaTermino(to),tipoSolicitud,noSecuencia
            var requests = (from sol in context.Solicitud
                            where sol.IdEmpresa == solicitud.IdEmpresa
                            && (sol.NoSecuencia == solicitud.NoSecuencia || solicitud.NoSecuencia == "" || solicitud.NoSecuencia == null)
                            && (sol.TipoSolicitud == solicitud.TipoSolicitud || solicitud.TipoSolicitud == null || solicitud.TipoSolicitud == "")
                            && ((sol.FechaCreacion >= solicitud.FechaInicio && sol.FechaCreacion <= solicitud.FechaTermino) || solicitud.FechaInicio == null || solicitud.FechaTermino == null)
                            select new
                           {
                               sol.Id,
                               sol.NoSecuencia,
                               FechaCreacion=sol.FechaCreacion,
                               FechaInicio=sol.FechaInicio,
                               HoraInicio=sol.HoraInicio,
                               FechaTermino=sol.FechaTermino,
                               HoraTermino = sol.HoraTermino,
                               atendidoPor = context.Usuario.Where(e=>e.Id == sol.IdUsuario).SingleOrDefault().CuentaUsuario,
                               cliente = context.Cliente.Where(e => e.Id == sol.IdCliente).SingleOrDefault().Nombre,
                               sol.TipoSolicitud,
                               sol.TipoServicio,
                               sol.Estado,
                               AprobadoPor = context.Usuario.Where(e => e.Id == sol.AprobadoPor).SingleOrDefault().CuentaUsuario,
                               sol.IdEmpresa
                           }).ToList();

            return new JsonResult(requests);
        }
    }
}
