﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models.classes;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly HelpDeskDBContext context;

        public BusinessController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }

        // GET: api/Business
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            List<Empresa> empresas = new List<Empresa>();
            var user = context.Usuario.Find(id);
            if (user.Acceso != "ROOT")
                empresas.Add(context.Empresa.Find(user.IdEmpresa));
            else
                empresas = context.Empresa.ToList();
            return new JsonResult(empresas.OrderByDescending(x=>x.Id));
        }

        // GET: api/Business/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Business
        [HttpPost]
        public JsonResult Post([FromBody] Empresa req)
        {
            ObjectResponse res;
            try
            {
                if (req.RazonSocial == null || req.RazonSocial == "" || req.Secuenciaticket == null || req.Secuenciaticket == "")
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
                req.Habilitado = true;
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.SaveChanges();

                var data = context.Empresa.Where(e => e.RazonSocial == req.RazonSocial
                && e.SectorComercial == req.SectorComercial && e.Rnc == req.Rnc && e.Telefono == req.Telefono
                && e.Correo == req.Correo && e.Contrasena == req.Contrasena && e.Url ==req.Url && e.Port ==req.Port
                && e.Host == req.Host && e.Direccion ==req.Direccion 
                && e.NoAutorizacion== req.NoAutorizacion && e.Secuenciaticket == req.Secuenciaticket).SingleOrDefault();
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

        // PUT: api/Business/5
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] Empresa req)
        {   
            ObjectResponse res;
            try
            {
                if (req.RazonSocial == null || req.RazonSocial == "" || req.Secuenciaticket == null || req.Secuenciaticket == "")
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
                var empresa = context.Empresa.Find(id);
                empresa.RazonSocial = req.RazonSocial;
                empresa.SectorComercial = req.SectorComercial;
                empresa.Rnc = req.Rnc;
                empresa.Telefono = req.Telefono;
                empresa.Correo = req.Correo;
                empresa.Contrasena = req.Contrasena;
                empresa.Url = req.Url;
                empresa.Host = req.Host;
                empresa.Port = req.Port;
                empresa.Direccion = req.Direccion;
                empresa.Image = req.Image;
                empresa.NoAutorizacion = req.NoAutorizacion;
                empresa.Secuenciaticket = req.Secuenciaticket;
                empresa.Habilitado = req.Habilitado;
                context.Entry(empresa).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                res = new ObjectResponse {
                    code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "has been updated successfully",
                    data =  null     
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