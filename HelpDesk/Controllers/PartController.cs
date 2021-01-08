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
    public class PartController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public PartController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        // GET: api/Part
        [HttpGet("{idSolicitud}/{idEmpresa}")]
        public JsonResult Get(int idSolicitud, int idEmpresa)
        {
            List<Piezas> parts = new List<Piezas>();
            try
            {
                parts = context.Piezas.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
            }
            catch (Exception)
            {

            }
            return new JsonResult(parts);
        }

        //// GET: api/Part/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Part
        [HttpPost]
        public JsonResult Post([FromBody] Piezas req)
        {
            ObjectResponse res;
            try
            {
                if (req.Descripcion == "" || req.Descripcion == null || req.Cantidad <= 0
                    || req.Cantidad == null || req.NoSerial == "" || req.NoSerial == null)
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

                context.Piezas.Add(req);
                context.SaveChanges();

                int idSolicitud = req.IdSolicitud;
                int idEmpresa = req.IdEmpresa;
                var data = context.Piezas.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
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

        // PUT: api/Part
        [HttpPut]
        public JsonResult Put([FromBody] Piezas req)
        {
            ObjectResponse res;
            try
            {
                if (req.Descripcion == "" || req.Descripcion == null || req.Cantidad <= 0
                    || req.Cantidad == null || req.NoSerial == "" || req.NoSerial == null)
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

                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChanges();

                int idSolicitud = req.IdSolicitud;
                int idEmpresa = req.IdEmpresa;
                var data = context.Piezas.Where(e => e.IdSolicitud == idSolicitud && e.IdEmpresa == idEmpresa).ToList();
                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "modified successfully",
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

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            ObjectResponse res;
            try
            {
                var req = context.Piezas.Find(id);
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                context.SaveChanges();

                res = new ObjectResponse
                {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "modified successfully",
                    data = null
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
