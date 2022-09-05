using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Core.Services
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(Solicitud, EmailMessage)> CreateTicket(Solicitud ticket)
        {
            EmailMessage emailMessage = null;
            var business = await _unitOfWork.BusinessRepository.GetById(ticket.IdEmpresa);

            ticket.IdUsuario = ticket.IdUsuario < 1 ? 0 : ticket.IdUsuario;
            ticket.FechaCreacion = DateTime.Now.Date;
            ticket.Habilitado = true;
            ticket.NoSecuencia = business.Secuenciaticket == string.Empty ? "1" : business.Secuenciaticket;
            business.Secuenciaticket = (Convert.ToInt32(ticket.NoSecuencia) + 1).ToString();
            await _unitOfWork.TicketRepository.Add(ticket);
            _unitOfWork.BusinessRepository.Update(business);
            await _unitOfWork.SaveChangeAsync();

            var customer = await _unitOfWork.CustomerRepository.GetById(ticket.IdCliente);
            var user = await _unitOfWork.UserRepository.GetById(ticket.IdUsuario);

            if (ticket.IdUsuario != 0)
            {
                var subject = $"HelpDesk Notification - Orden No.{ticket.NoSecuencia}";

                var discrepancia = ticket.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                var body = $@"<div>
                                {business.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{user.Nombre}</i></b><br>
                                Fecha: <b><i>{ticket.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{ticket.NoSecuencia}</i></b><br>
                                Clientes: <b><i>{customer.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{ticket.Descripcion}</i></b><br>
                            </div>";
                
                emailMessage = new EmailMessage
                {
                    Body = body,
                    Subject = subject,
                    ReceptorEmail = user.Correo,
                    Host = business.Host,
                    Port = Convert.ToInt32(business.Port),
                    EmisorEmail = business.Correo,
                    PasswordEmail = business.Contrasena
                };
            }
            return (ticket, emailMessage);
        }

        public async Task<Metric> GetMetricsByUserId(int userId)
        {
            var user = await _unitOfWork.UserRepository.GetById(userId);
            var ticketsByUser = _unitOfWork.TicketRepository.GetTicketsByUserId(user.Id);
            var ticketsByBusiness = _unitOfWork.TicketRepository.GetAll().Where(x => x.IdEmpresa == user.IdEmpresa);
            var abiertos = ticketsByUser.Where(e => e.Estado.ToUpper() == "ABIERTO").Count();
            var enproceso = ticketsByUser.Where(e =>e.Estado.ToUpper() == "EN PROCESO").Count();
            var completados = ticketsByUser.Where(e => e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).Count();
            var costumers = _unitOfWork.CustomerRepository.GetByBusinessId(user.IdEmpresa, true).ToList().Count();
            var waitingToAttend = user.Acceso == "TECNICO" ? 0 : ticketsByBusiness.Where(e => e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO").Count();
            var waiting2ToAttend = user.Acceso == "TECNICO" ? 0 : ticketsByBusiness.Where(e => e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO" && e.TipoSolicitud == "Servicio Taller").Count();
            var waiting3ToAttend = user.Acceso == "TECNICO" ? 0 : ticketsByBusiness.Where(e => e.IdUsuario < 1 && e.Estado.ToUpper() != "COMPLETADO" && e.TipoSolicitud == "Servicio a Domicilio").Count();
            var finished = user.Acceso == "TECNICO" ? 0 : ticketsByBusiness.Where(e =>e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor != null).Count();
            var finishedToAttend = user.Acceso == "TECNICO" ? 0 : ticketsByBusiness.Where(e => e.Estado.ToUpper() == "COMPLETADO" && e.AprobadoPor == null).Count();

            var metrics = new Metric
            {
                Abiertos = abiertos,
                Enproceso = enproceso,
                Completados = completados,
                Costumers = costumers,
                WaitingToAttend = waitingToAttend,
                Waiting2ToAttend = waiting2ToAttend,
                Waiting3ToAttend = waiting3ToAttend,
                Finished = finished,
                FinishedToAttend = finishedToAttend
            };
            return metrics;
        }

        public IEnumerable<SummaryInformation> GetSummaryInformation(Solicitud ticket)
        {
            var tickets = _unitOfWork.TicketRepository.GetAll().ToList();
            var users = _unitOfWork.UserRepository.GetAll().ToList();
            var customers = _unitOfWork.CustomerRepository.GetAll().ToList();
            var response = (from sol in tickets
                            where sol.IdEmpresa == ticket.IdEmpresa
                            && (sol.NoSecuencia == ticket.NoSecuencia || ticket.NoSecuencia == "" || ticket.NoSecuencia == null)
                            && (sol.TipoSolicitud == ticket.TipoSolicitud || ticket.TipoSolicitud == null || ticket.TipoSolicitud == "")
                            && ((sol.FechaCreacion >= ticket.FechaInicio && sol.FechaCreacion <= ticket.FechaTermino) || ticket.FechaInicio == null || ticket.FechaTermino == null)
                            select new SummaryInformation
                            {
                                Id = sol.Id,
                                NoSecuencia = sol.NoSecuencia,
                                FechaCreacion = sol.FechaCreacion,
                                FechaInicio = sol.FechaInicio,
                                HoraInicio = sol.HoraInicio,
                                FechaTermino = sol.FechaTermino,
                                HoraTermino = sol.HoraTermino,
                                AtendidoPor = users.Where(e => e.Id == sol.IdUsuario).SingleOrDefault() != null ? users.Where(e => e.Id == sol.IdUsuario).SingleOrDefault().CuentaUsuario : null,
                                Cliente = customers.Where(e => e.Id == sol.IdCliente).SingleOrDefault() != null ? customers.Where(e => e.Id == sol.IdCliente).SingleOrDefault().Nombre : null,
                                TipoSolicitud = sol.TipoSolicitud,
                                TipoServicio = sol.TipoServicio,
                                Estado = sol.Estado,
                                AprobadoPor = users.Where(e => e.Id == sol.AprobadoPor).SingleOrDefault() != null ? users.Where(e => e.Id == sol.AprobadoPor).SingleOrDefault().CuentaUsuario : null,
                                IdEmpresa = sol.IdEmpresa
                            });
            
            return response;
        }

        public async Task<IEnumerable<SolicitudT>> GetTicketsByIdAndCondition(int id, string option)
        {
            if (option is null)
            {
                throw new Exception("the option is required");
            }
            option = option.ToLower();
            var container = new List<Solicitud>();

            if (option == "unique")
            {
                var tickets = _unitOfWork.TicketRepository.GetTicketsByUserId(id);
                container.AddRange(tickets);
            }
            else if (option == "*" || option == "all")
            {
                var user = await _unitOfWork.UserRepository.GetById(id);
                switch (user.Acceso)
                {
                    case "ROOT":
                    case "ADMINISTRADOR":
                    case "MODERADOR":
                        var list = _unitOfWork.TicketRepository.GetAll().Where(u => u.IdEmpresa == user.IdEmpresa).OrderByDescending(x => x.Id);
                        container.AddRange(list);
                        break;
                    case "TECNICO":
                    default:
                        break;
                }
            }
            var customers = _unitOfWork.CustomerRepository.GetAll();
            var devices = _unitOfWork.DeviceRepository.GetAll();

            var containerTicket = new List<SolicitudT>();

            container.ForEach(e =>
            {
                var customer = customers.Where(x => x.Id == e.IdCliente).First();
                var codes1 = customer.Nombre + ", " + customer.Contacto;
                var eq = devices.Where(x => x.Id == e.IdCliente).ToList();
                var codes2 = "";
                eq.ForEach(r =>
                {
                    codes2 = codes2 + r.NoSerial + ", ";
                });

                containerTicket.Add(new SolicitudT
                {
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
                    cliente = customer.Nombre,
                    claves = codes1 + ", " + codes2
                });
            });
            return containerTicket.AsEnumerable();
        }

        public async Task<(Solicitud, EmailMessage)> UpdateTicket(Solicitud ticket, string action)
        {
            EmailMessage emailMessage = null;
            if (string.IsNullOrEmpty(action))
            {
                throw new Exception("action is required. Please select an action.");
            }
            //ticket = await _unitOfWork.TicketRepository.GetById(ticket.Id);
            var business = await _unitOfWork.BusinessRepository.GetById(ticket.IdEmpresa);
            var user = await _unitOfWork.UserRepository.GetById(ticket.IdUsuario);
            var customer = await _unitOfWork.CustomerRepository.GetById(ticket.IdCliente);

            if (action is "ESTADO")
            {
                DateTime today = DateTime.Now.Date;
                TimeSpan time = DateTime.Now.TimeOfDay;
                ticket.Estado = ticket.Estado;
                switch (ticket.Estado.ToUpper())
                {
                    case "ABIERTO":
                        ticket.AprobadoPor = null;
                        break;
                    case "EN PROCESO":
                        ticket.AprobadoPor = null;
                        break;
                    case "COMPLETADO":
                        if (ticket.FechaInicio is null || ticket.HoraInicio is null
                            || ticket.FechaTermino is null || ticket.HoraTermino is null)
                        {
                            action = "NOCHANGEPLEASE";
                            throw new Exception("Favor completar los campos correspondientes a las fechas y horas");
                            //res = new ObjectResponse
                            //{
                            //    code = "5",
                            //    title = "Datos requeridos",
                            //    icon = "warning",
                            //    message = "Favor completar los campos correspondientes a las fechas y horas",
                            //    data = context.Solicitudes.Find(ticket.Id)
                            //};
                            //return new JsonResult(res);
                        }
                        else if (ticket.FechaInicio > ticket.FechaTermino ||
                        (ticket.FechaInicio == ticket.FechaTermino && ticket.HoraInicio > ticket.HoraTermino))
                        {
                            action = "NOCHANGEPLEASE";
                            throw new Exception("Favor verificar la fecha y hora de inicio con la fecha y hora de termino. Este ultimo debe ser posterior a la fecha de inicio.");
                            //res = new ObjectResponse
                            //{
                            //    code = "5",
                            //    title = "Fecha y hora no coinciden",
                            //    icon = "warning",
                            //    message = "Favor verificar la fecha y hora de inicio con la fecha y hora de termino. Este ultimo debe ser posterior a la fecha de inicio.",
                            //    data = context.Solicitudes.Find(ticket.Id)
                            //};
                            //return new JsonResult(res);
                        }
                        break;
                    default:
                        break;
                }

            }
            else if (action is "USUARIO")
            {
                //ticket.IdUsuario = ticket.IdUsuario;

                var subject = $"Orden asignada No.{ticket.NoSecuencia}";
                var discrepancia = ticket.TipoSolicitud == "Servicio Taller" ? "Observaciones" : "Falla";
                var body = $@"<div>
                                {business.RazonSocial}<br>
                                Orden de servicio técnico asignado: <b><i>{user.Nombre}</i></b><br>
                                Fecha: <b><i>{ticket.FechaCreacion.ToString("dd/MM/yyyy")}</i></b><br>
                                Orden: <b><i>{ticket.NoSecuencia}</i></b><br>
                                Clientes: <b><i>{customer.Nombre}</i></b><br>
                                {discrepancia}: <b><i>{ticket.Descripcion}</i></b><br>
                            </div>";

                emailMessage = new EmailMessage
                {
                    Body = body,
                    Subject = subject,
                    ReceptorEmail = user.Correo,
                    Host = business.Host,
                    Port = Convert.ToInt32(business.Port),
                    EmisorEmail = business.Correo,
                    PasswordEmail = business.Contrasena
                };
                //var resp = MailClient.Send(business.Host, Convert.ToInt32(business.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                //true, business.Correo, business.Contrasena, user.Correo, subject, body, true);
            }
            else
            {
                ticket.FechaInicio = ticket.FechaInicio;
                ticket.HoraInicio = ticket.HoraInicio;
                ticket.FechaTermino = ticket.FechaTermino;
                ticket.HoraTermino = ticket.HoraTermino;
                ticket.AprobadoPor = ticket.AprobadoPor;

                ticket.Descripcion = ticket.Descripcion;
                ticket.TipoServicio = ticket.TipoServicio;
                ticket.TipoSolicitud = ticket.TipoSolicitud;
                ticket.IdCliente = ticket.IdCliente;


                if (action == "APROBAR")
                {
                    List<Equipo> equipos = _unitOfWork.DeviceRepository.GetAll().Where(e => e.IdEmpresa == ticket.IdEmpresa && e.IdSolicitud == ticket.Id && e.Habilitado == true).ToList();
                    var subject = $"{business.RazonSocial} - Orden No.{ticket.NoSecuencia}";
                    var activities = _unitOfWork.TraceRepository.GetAll().Where(s => s.IdEmpresa == ticket.IdEmpresa && s.IdSolicitud == ticket.Id && s.Etiquetado == true && s.Habilitado == true).ToList();
                    var style = "";

                    var body = style + $@"
                                    <!-- HEADER -->
                                    <header>
                                      <div class='row border-bottom border-dark pb-2'>
                                        <div class='col-6'>
                                          <img src='{"data:image/jpeg;base64," + Convert.ToBase64String(business.Image)}' class='logo' alt='{business.RazonSocial}'/>
                                        </div>

                                      </div>
                                      <div class='row py-2'>

                                        <div class='col-6'>";
                    var c = business.Direccion.Split('\n').Count();
                    for (int i = 0; i < c; i++)
                    {
                        body = body + $"<p class='info'>{business.Direccion.Split('\n')[i]}</p>";

                    }

                    body = body + $@"<p class='info'>Tel: {business.Telefono}</p>
                                          <p class='info'>E-mail: {business.Correo}</p>
                                        </div>
                                        <div class='col-6 d-flex justify-content-end align-items-top py-2'>
                                          <p class='info'>RNC: {business.Rnc}</p>
                                        </div>
                                      </div>
                                    </header>
                                <!-- HEADER -->";

                    body = body + $@"<div class='col-6 d-flex justify-content-end align-items-center'>
                                          <div class='service-order'>Les informamos que la orden de servicio No. <span class='service-order--number'>{ticket.NoSecuencia}</span> ha sido concluida.</div>
                                        </div>";
                    if (equipos.Count() > 0)
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
                        equipos.ForEach(e =>
                        {
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
                            activities.ForEach(a =>
                            {
                                body += $@"<li>{a.Texto}</li>";
                            });
                            body += @"</ul>";
                        }
                    }

                    emailMessage = new EmailMessage
                    {
                        Body = body,
                        Subject = subject,
                        ReceptorEmail = customer.Correo,
                        Host = business.Host,
                        Port = Convert.ToInt32(business.Port),
                        EmisorEmail = business.Correo,
                        PasswordEmail =business.Contrasena
                    };

                    //var resp = MailClient.Send(business.Host, Convert.ToInt32(business.Port), System.Net.Mail.SmtpDeliveryMethod.Network, false,
                    //        true, business.Correo, business.Contrasena, customer.Correo, subject, body, true);
                }
            }
            _unitOfWork.TicketRepository.Update(ticket);//context.Entry(ticket).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            if (action != "NOCHANGEPLEASE") await _unitOfWork.SaveChangeAsync();

            //var ticket_res = context.Solicitudes.Find(ticket.Id);


            //res = new ObjectResponse
            //{
            //    code = "1",
            //    title = "Saved",
            //    icon = "success",
            //    message = "has been updated successfully",
            //    data = ticket_res
            //};

            return (ticket, emailMessage);
        }
    }
}
