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
    public class DeviceController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public DeviceController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        // GET: api/Device
        [HttpGet("{idSolicitud}/{idEmpresa}")]
        public JsonResult Get(int idSolicitud, int idEmpresa)
        {
            List<Equipo> equipos = new List<Equipo>();
            try
            {
                 equipos = context.Equipo.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
            }
            catch (Exception)
            {

            }
            return new JsonResult(equipos);
        }

        //// GET: api/Device/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Device
        [HttpPost("[action]")]
        public JsonResult PostOne([FromBody]Equipo req)
        {
            ObjectResponse res;
            try
            {
                if (req.Marca == "" || req.Marca == null ||
                req.Modelo == "" || req.Modelo == null ||
                req.NoSerial == "" || req.NoSerial == null)
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "Field(s) required!",
                        data = null
                    };
                    return new JsonResult(res);
                }
                req.Habilitado = true;

                context.Equipo.Add(req);
                context.SaveChanges();

                int idSolicitud = req.IdSolicitud;
                int idEmpresa = req.IdEmpresa;
                var data = context.Equipo.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
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

        [HttpPost("[action]")]
        public JsonResult PostArray([FromBody]List<Equipo> req)
        {
            ObjectResponse res;
            try
            {            
                if (req.Where(e=> e.Marca =="" || e.Marca == null ||
                e.Descripcion == "" || e.Descripcion == null ||
                e.Modelo =="" || e.Modelo == null ||
                e.NoSerial == "" || e.NoSerial == null).Count() > 0)
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "Validation errors",
                        icon = "warning",
                        message = "Field(s) required!",
                        data = null
                    };
                    return new JsonResult(res);
                }
                req.ForEach(e => {
                    e.Id = 0;
                    e.Habilitado = true;
                });
                context.Equipo.AddRange(req);
                context.SaveChanges();        

                int idSolicitud = req[0].IdSolicitud;
                int idEmpresa = req[0].IdEmpresa;
                var data = context.Equipo.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
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

        // PUT: api/Device/5
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
