using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Infrastructure.Data;
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
            //List<Solicitudes> solicitudes = new List<Solicitudes>();
            List<SolicitudT> solicitudes = new List<SolicitudT>();

            if (option.ToLower() == "unique")
            {
                var array = context.Solicitudes.Where(s => s.IdUsuario == idUser).OrderByDescending(s=>s.Id);
                //solicitudes.AddRange(array);
                array.ToList().ForEach(e =>
                {
                    var codigos = context.Clientes.Find(e.IdCliente).Nombre +", "+ context.Clientes.Find(e.IdCliente).Contacto;
                    var eq = context.Equipos.Where(a => a.IdSolicitud == e.Id).ToList();
                    var codigos2 = "";// eq.Select(r => r.NoSerial).Join(",");
                    eq.ForEach(r =>
                    {
                        codigos2 = codigos2 + r.NoSerial +", ";
                    });


                    solicitudes.Add(new SolicitudT {
                        Id = e.Id,
                        NoSecuencia = e.NoSecuencia,
                        FechaCreacion = e.FechaCreacion,
                        FechaInicio = e.FechaInicio,
                        HoraInicio = e.HoraInicio,
                        HoraTermino = e.HoraTermino,
                        FechaTermino = e.FechaTermino,
                        IdUsuario = e.IdUsuario,
                        IdCliente = e.IdCliente,
                        TipoSolicitud = e.TipoSolicitud,
                        TipoServicio = e.TipoServicio,
                        Estado = e.Estado,
                        IdEmpresa = e.IdEmpresa,
                        Descripcion = e.Descripcion,
                        AprobadoPor = e.AprobadoPor,
                        Habilitado = e.Habilitado,
                        cliente = context.Clientes.Find(e.IdCliente).Nombre,
                        claves = codigos + ", " + codigos2
                    });
                });
            }
            else if (option == "*" || option.ToLower() == "all")
            {                                            
                Usuario usuario = context.Usuarios.Find(idUser);
                switch (usuario.Acceso)
                {
                    case "ROOT":
                    //break;
                    case "ADMINISTRADOR":
                        //break;
                    case "MODERADOR":
                        //solicitudes.AddRange(context.Solicitudes.Where(u => u.IdEmpresa == usuario.IdEmpresa).OrderByDescending(x => x.Id));
                        var list = context.Solicitudes.Where(u => u.IdEmpresa == usuario.IdEmpresa).OrderByDescending(x => x.Id);
                        list.ToList().ForEach(e =>
                        {
                            var codigos = context.Clientes.Find(e.IdCliente).Nombre + ", " + context.Clientes.Find(e.IdCliente).Contacto;
                            var eq = context.Equipos.Where(a => a.IdSolicitud == e.Id).ToList();
                            var codigos2 = "";// eq.Select(r => r.NoSerial).Join(",");
                            eq.ForEach(r =>
                            {
                                codigos2 = codigos2 + r.NoSerial + ", ";
                            });


                            solicitudes.Add(new SolicitudT
                            {
                                Id = e.Id,
                                NoSecuencia = e.NoSecuencia,
                                FechaCreacion = e.FechaCreacion,
                                FechaInicio =e.FechaInicio,
                                HoraInicio = e.HoraInicio,
                                HoraTermino = e.HoraTermino,
                                FechaTermino = e.FechaTermino,
                                IdUsuario = e.IdUsuario,
                                IdCliente = e.IdCliente,
                                TipoSolicitud = e.TipoSolicitud,
                                TipoServicio = e.TipoServicio,
                                Estado = e.Estado,
                                IdEmpresa = e.IdEmpresa,
                                Descripcion = e.Descripcion,
                                AprobadoPor = e.AprobadoPor,
                                Habilitado = e.Habilitado,
                                cliente = context.Clientes.Find(e.IdCliente).Nombre,
                                claves = codigos + ", " + codigos2
                            });
                        });
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
                Empresa empresa = context.Empresas.Find(req.IdEmpresa);

                req.IdUsuario = req.IdUsuario < 1 ? 0 : req.IdUsuario;
                req.FechaCreacion = DateTime.Now.Date;
                req.Habilitado = true;
                req.NoSecuencia = empresa.Secuenciaticket == string.Empty ? "1" : empresa.Secuenciaticket;
                empresa.Secuenciaticket = (Convert.ToInt32(req.NoSecuencia)+1).ToString();
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.Entry(empresa).State = Microsoft.EntityFrameworkCore.EntityState.Modified;  
                context.SaveChanges();


                Cliente cliente = context.Clientes.Find(req.IdCliente);
                
                if(req.IdUsuario != 0 )
                {
                    Usuario usuario = context.Usuarios.Where(u => u.IdEmpresa == req.IdEmpresa && u.Id == req.IdUsuario).SingleOrDefault();
                    var subject = $"HelpDesk Notification - Orden No.{req.NoSecuencia}";

                    var discrepancia = req.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                    var body = $@"<div>
                                {empresa.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{usuario.Nombre}</i></b><br>
                                Fecha: <b><i>{req.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{req.NoSecuencia}</i></b><br>
                                Clientes: <b><i>{cliente.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{req.Descripcion}</i></b><br>
                            </div>";

                    var resp = MailClient.Send(empresa.Host, Convert.ToInt32(empresa.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                    true, empresa.Correo, empresa.Contrasena, usuario.Correo, subject, body, true);

                }

                var data = context.Solicitudes.Where(e => e.NoSecuencia == req.NoSecuencia).LastOrDefault();
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
        //[HttpPut("{toDo}")]
        [HttpPost("[action]/{toDo}")]
        public JsonResult /*Put*/EDIT(string toDo, [FromBody] Solicitud req)
        {
            ObjectResponse res;
            try
            {
                if (req.IdEmpresa == null || req.IdEmpresa <= 0|| /*req.IdUsuario == null || req.IdUsuario <= 0 ||*/ req.IdCliente == null || req.IdCliente <= 0
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
                Solicitud ticket = context.Solicitudes.Find(req.Id);
                Empresa empresa = context.Empresas.Find(req.IdEmpresa);
                Usuario usuario = context.Usuarios.Find(req.IdUsuario);
                Cliente cliente = context.Clientes.Find(req.IdCliente);

                if (toDo is "ESTADO")
                {
                    DateTime today = DateTime.Now.Date;
                    TimeSpan time = DateTime.Now.TimeOfDay;
                    ticket.Estado = req.Estado;
                    switch (req.Estado.ToUpper())
                    {
                        case "ABIERTO":
                            //ticket.FechaTermino = null;
                            //ticket.HoraTermino = null;
                            ticket.AprobadoPor = null;
                            break;
                        case "EN PROCESO":
                            //ticket.FechaInicio = req.FechaInicio ==null?today: req.FechaInicio;
                            //ticket.HoraInicio = req.FechaInicio==null? time: req.HoraInicio;
                            //ticket.FechaTermino = null;
                            //ticket.HoraTermino = null;
                            ticket.AprobadoPor = null;
                            break;
                        case "COMPLETADO":
                            //if(ticket.FechaInicio is null) ticket.FechaInicio = today;
                            //if (ticket.HoraInicio is null) ticket.HoraInicio = time;
                            //ticket.FechaTermino = today;
                            //ticket.HoraTermino = time;
                            if (ticket.FechaInicio is null || ticket.HoraInicio is null
                                || ticket.FechaTermino is null || ticket.HoraTermino is null)
                            {
                                toDo = "NOCHANGEPLEASE";

                                res = new ObjectResponse
                                {
                                    code = "5",
                                    title = "Datos requeridos",
                                    icon = "warning",
                                    message = "Favor completar los campos correspondientes a las fechas y horas",
                                    data = context.Solicitudes.Find(ticket.Id)
                                };
                                return new JsonResult(res);
                            }
                            else if ( ticket.FechaInicio > ticket.FechaTermino ||
                            (ticket.FechaInicio == ticket.FechaTermino && ticket.HoraInicio > ticket.HoraTermino)   )
                                {
                                    toDo = "NOCHANGEPLEASE";

                                    res = new ObjectResponse
                                    {
                                        code = "5",
                                        title = "Fecha y hora no coinciden",
                                        icon = "warning",
                                        message = "Favor verificar la fecha y hora de inicio con la fecha y hora de termino. Este ultimo debe ser posterior a la fecha de inicio.",
                                        data = context.Solicitudes.Find(ticket.Id)
                                    };
                                    return new JsonResult(res);
                                }
                            break;
                        default:
                            break;
                    }
                    
                }
                else if(toDo is "USUARIO")
                {
                    ticket.IdUsuario = req.IdUsuario;

                    var subject = $"Orden asignada No.{req.NoSecuencia}";
                    var discrepancia = req.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                    var body = $@"<div>
                                {empresa.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{usuario.Nombre}</i></b><br>
                                Fecha: <b><i>{req.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{req.NoSecuencia}</i></b><br>
                                Clientes: <b><i>{cliente.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{req.Descripcion}</i></b><br>
                            </div>";

                    //Empresas empresa = context.Empresas.Find(req.IdEmpresa);
                    //Usuarios usuario = context.Usuarios.Find(req.IdUsuario);

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

                    ticket.Descripcion = req.Descripcion;
                    ticket.TipoServicio = req.TipoServicio;
                    ticket.TipoSolicitud = req.TipoSolicitud;
                    ticket.IdCliente = req.IdCliente;

                    
                    if (toDo == "APROBAR")
                    {
                        List<Equipo> equipos = context.Equipos.Where(e => e.IdEmpresa == req.IdEmpresa && e.IdSolicitud == req.Id && e.Habilitado == true).ToList();
                        var subject = $"{empresa.RazonSocial} - Orden No.{req.NoSecuencia}";
                        var activities = context.Seguimientos.Where(s => s.IdEmpresa == req.IdEmpresa && s.IdSolicitud == req.Id && s.Etiquetado == true && s.Habilitado == true).ToList();
                        var style = "";
                        //var style = @" <meta charset='UTF-8'>
                        //           <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@4.5.3/dist/css/bootstrap.min.css' integrity='sha384 -TX8t27EcRE3e/ihU7zmQxVncDAy5uIKz4rEkgIXeMed4M0jlfIDPvg6uqKI2xXr2' crossorigin='anonymous'>
                        //           <style>
                        //            .logo {
                        //                width: 100%;
                        //                height: auto;
                        //                max-width: 60%;
                        //            }

                        //            .info {
                        //                color: black;
                        //                font-size: 12px;
                        //                font-style: italic;
                        //                font-weight: bold;
                        //                line-height: 0;
                        //            }

                        //            .service-order {
                        //                background-color: #F8F9FA;
                        //                border-radius: 6px;
                        //                padding: 10px;
                        //            }

                        //            .custom-border {
                        //                border-top: 0;
                        //                border-right: 0;
                        //                border-left: 0;
                        //                border-radius: 0;
                        //            }

                        //            .table-bordered,
                        //            .table-bordered td, .table-bordered th {
                        //                border: 1px solid black!important;
                        //            }

                        //            .table td, .table th {
                        //                padding: .25rem!important;
                        //            }

                        //            td {
                        //                height: 25px;
                        //            }

                        //            .firma {
                        //                border-collapse: separate; border-spacing: 50px; 
                        //            }

                        //            .border-bottom {
                        //                border-color: black!important;
                        //            }


                        //            table {
                        //              font-family: arial, sans-serif;
                        //              border-collapse: collapse;
                        //              width: 100%;
                        //            }

                        //            td, th {
                        //              border: 1px solid #dddddd;
                        //              text-align: left;
                        //              padding: 8px;
                        //            }

                        //            tr:nth-child(even) {
                        //              background-color: #dddddd;
                        //            }


                        //            </style>";

                        var body = style + $@"
                                    <!-- HEADER -->
                                    <header>
                                      <div class='row border-bottom border-dark pb-2'>
                                        <div class='col-6'>
                                          <img src='{"data:image/jpeg;base64,"+Convert.ToBase64String(empresa.Image)}' class='logo' alt='{empresa.RazonSocial}'/>
                                        </div>

                                      </div>
                                      <div class='row py-2'>
                                      
                                        <div class='col-6'>";
                        var c = empresa.Direccion.Split('\n').Count();
                                        for (int i = 0; i < c; i++)
			                            {
                            body = body + $"<p class='info'>{empresa.Direccion.Split('\n')[i]}</p>";

                                        }

                        body = body+ $@"<p class='info'>Tel: {empresa.Telefono}</p>
                                          <p class='info'>E-mail: {empresa.Correo}</p>
                                        </div>
                                        <div class='col-6 d-flex justify-content-end align-items-top py-2'>
                                          <p class='info'>RNC: {empresa.Rnc}</p>
                                        </div>
                                      </div>
                                    </header>
                                <!-- HEADER -->";

                        body = body + $@"<div class='col-6 d-flex justify-content-end align-items-center'>
                                          <div class='service-order'>Les informamos que la orden de servicio No. <span class='service-order--number'>{ticket.NoSecuencia}</span> ha sido concluida.</div>
                                        </div>";
                        if (equipos.Count() >0)
                        {
                            body += $@"
                              <h2>Equipos</h2>
                        <table border='1'>
                          <tr>
                            <th >Descripción</th>
                            <th>Marca</th>
                            <th >Modelo</th>
                            <th >No. Serial</th>
                          </tr>
                          <tr>";
                            equipos.ForEach(e => {
                                body += $@"                            
                             <td>{e.Descripcion}</td>
                            <td >{e.Marca}</td>
                            <td >{e.Modelo}</td>
                            <td>{e.NoSerial}</td>";
                            });

                            body += @"</tr>
                        </table>";

                            if (activities.Count > 0)
                            {
                               

                                body += @"<h2>Actividades Realizadas</h2>
                                            <ul>";
                                activities.ForEach(a => {
                                    body += $@"<li>{a.Texto}</li>";
                                });
                                body += @"</ul>";
                            }
                        }
                        //"albertparedesdo@gmail.com"
                        var resp = MailClient.Send(empresa.Host, Convert.ToInt32(empresa.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                                true, empresa.Correo, empresa.Contrasena, cliente.Correo, subject,body, true);
                    }
                }
                context.Entry(ticket).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                if (toDo != "NOCHANGEPLEASE") context.SaveChanges();

                var ticket_res = context.Solicitudes.Find(req.Id);

                
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
        //                            Clientes:<b><i>Mix Viajes & Cruceros</i></b><br>
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
                Usuario usuario = await context.Usuarios.FindAsync(idUser);
                var tickets = context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario == usuario.Id).ToList();
                var abiertos = tickets.Where(e=>e.Estado.ToUpper() =="ABIERTO").Count();
                //var enproceso = tickets.Where(e => e.Estado.ToUpper() != "ABIERTO" || e.Estado.ToUpper() == "EN PROCESO" || (e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null)).Count();//deprecated
                var enproceso = tickets.Where(e => /*e.Estado.ToUpper() != "ABIERTO" ||*/ e.Estado.ToUpper() == "EN PROCESO" /*|| (e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null)*/).Count();
                var completados = tickets.Where(e => e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).Count();
                var costumers = context.Clientes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.Habilitado == true).ToList().Count();
                var waitingToAttend = usuario.Acceso == "TECNICO"   ? 0 : context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO").ToList().Count();
                var waiting2ToAttend = usuario.Acceso == "TECNICO" ? 0 : context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO" && e.TipoSolicitud =="Servicio Taller").ToList().Count();
                var waiting3ToAttend = usuario.Acceso == "TECNICO" ? 0 : context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO" && e.TipoSolicitud == "Servicio a Domicilio").ToList().Count();
                var finished = usuario.Acceso == "TECNICO" ? 0 : context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).ToList().Count();
                var finishedToAttend = usuario.Acceso == "TECNICO" ? 0 : context.Solicitudes.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null).ToList().Count();

                object data = new
                {
                    abiertos,
                    enproceso,
                    completados,
                    costumers,
                    waitingToAttend,
                    waiting2ToAttend,
                    waiting3ToAttend,
                    finished,//(deprecated)
                    finishedToAttend
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
            var requests = (from sol in context.Solicitudes
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
                               atendidoPor = context.Usuarios.Where(e=>e.Id == sol.IdUsuario).SingleOrDefault().CuentaUsuario,
                               cliente = context.Clientes.Where(e => e.Id == sol.IdCliente).SingleOrDefault().Nombre,
                               sol.TipoSolicitud,
                               sol.TipoServicio,
                               sol.Estado,
                               AprobadoPor = context.Usuarios.Where(e => e.Id == sol.AprobadoPor).SingleOrDefault().CuentaUsuario,
                               sol.IdEmpresa
                           }).ToList();

            return new JsonResult(requests);
        }
    }
}
