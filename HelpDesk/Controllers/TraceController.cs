using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.Entities;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Models;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraceController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public TraceController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        // GET: api/Trace
        [HttpGet("{idSolicitud}/{idEmpresa}")]
        public JsonResult Get(int idSolicitud, int idEmpresa)
        {
            //List<Seguimientos> seguimientos = new List<Seguimientos>();
            ObjectResponse res = new ObjectResponse();
            try
            {
                //seguimientos = context.Seguimientos.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
                var traces = from seguimiento in context.Seguimientos
                             join solicitud in context.Solicitudes on seguimiento.IdSolicitud equals solicitud.Id
                             join usuario in context.Usuarios on seguimiento.IdUsuario equals usuario.Id
                             where solicitud.Id == idSolicitud && solicitud.IdEmpresa == idEmpresa
                             select new
                             {
                                 seguimiento.Id,
                                 seguimiento.IdEmpresa,
                                 seguimiento.IdSolicitud,
                                 seguimiento.IdUsuario,
                                userName = usuario.Nombre,
                                userImage = usuario.Image,
                                 seguimiento.Texto,
                                 seguimiento.Fecha,
                                 seguimiento.Hora,
                                 seguimiento.Habilitado,
                                 seguimiento.Favorito,
                                 seguimiento.Etiquetado

                             };

                res.code = "1";
                res.title = "success";
                res.data = traces.ToList();

            }
            catch (Exception e)
            {
                res.code = "0";
                res.title = "error";
                res.message = e.Message;
                res.data = null;

            }
            return new JsonResult(res);
        }

        //// GET: api/Trace/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Trace
        [HttpPost]
        public JsonResult Post([FromBody] Seguimiento req)
        {
            ObjectResponse res;
            try
            {
                if (req.Texto == null || req.Texto == "" || req.IdEmpresa == null || req.IdEmpresa <= 0
                    || req.IdSolicitud == null || req.IdSolicitud <= 0 || req.IdUsuario == null 
                    || req.IdUsuario <= 0)
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "'Some fields are required'",
                        data = null
                    };
                    return new JsonResult(res);
                }
                req.Fecha = DateTime.Now.Date;
                req.Hora = DateTime.Now.TimeOfDay;
                req.Favorito = false;
                req.Etiquetado = false;
                req.Habilitado = true;
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.SaveChanges();

                var data = context.Seguimientos.Where(e => e.IdSolicitud == req.IdSolicitud
                && e.IdEmpresa == req.IdEmpresa).ToList();
                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "has been saved successfully",
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

        // PUT: api/Trace/5
        //[HttpPut]
        [HttpPost("[action]")]
        public JsonResult Put([FromBody] Seguimiento req)
        {
            ObjectResponse res;
            try
            {
                if (req.Texto == null || req.Texto == "" || req.IdEmpresa == null || req.IdEmpresa <= 0
                    || req.IdSolicitud == null || req.IdSolicitud <= 0 || req.IdUsuario == null
                    || req.IdUsuario <= 0)
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "'Nombre is required'",
                        data = null
                    };
                    return new JsonResult(res);
                }
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "has been updated successfully",
                    data = null
                };
                context.SaveChangesAsync();
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
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
