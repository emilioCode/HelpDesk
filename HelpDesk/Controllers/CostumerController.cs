﻿using System;
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
    public class CostumerController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public CostumerController(HelpDeskDBContext _context)
        {
            this.context = _context; 
        }

        // GET: api/Costumer/1/1
        [HttpGet("{idUserOridClient}/{option}")]
        public JsonResult Get(int idUserOridClient, string option=null)
        {
            List<Cliente> clientes = new List<Cliente>();
           
            if (option.ToLower() == "unique")
            {
                Cliente cliente = context.Cliente.Find(idUserOridClient);
                clientes.Add(cliente);
            }
            else if (option == "*" || option.ToLower() == "all")
            {
                Usuario usuario = context.Usuario.Find(idUserOridClient);
                switch (usuario.Acceso)
                {
                    case "ROOT":
                        //break;
                    case "ADMINISTRADOR":
                        clientes.AddRange(context.Cliente.Where(u => u.IdEmpresa == usuario.IdEmpresa).OrderByDescending(x => x.Id));
                        break;
                    case "MODERADOR":
                        //break;
                    case "TECNICO":
                        //break;
                    default:
                        clientes.AddRange(context.Cliente.Where(u => u.IdEmpresa == usuario.IdEmpresa && u.Habilitado == true).OrderByDescending(x => x.Id));
                        break;
                }
            }
            return new JsonResult(clientes);
        }

        //// GET: api/Costumer/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Costumer
        [HttpPost]
        public JsonResult Post([FromBody] Cliente req)
        {
            ObjectResponse res;
            try
            {
                if (req.Nombre == null || req.Nombre == "" || req.IdEmpresa == null || req.IdEmpresa <= 0)
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
                req.Habilitado = true;
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.SaveChanges();

                var data = context.Cliente.Where(e => e.Nombre == req.Nombre
                && e.Contacto == req.Contacto && e.Telefono == req.Telefono && e.Extension == req.Extension
                && e.Rnc == req.Rnc && e.Departamento == req.Departamento && e.Correo == req.Correo
                && e.Direccion == req.Direccion && e.IdEmpresa == req.IdEmpresa).SingleOrDefault();
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

        // PUT: api/Costumer/5
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] Cliente req)
        {
            ObjectResponse res;
            try
            {
                if (req.Nombre == null || req.Nombre == "" || req.IdEmpresa == null || req.IdEmpresa <= 0)
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
                var cliente = context.Cliente.Find(id);
                cliente.Nombre = req.Nombre == "" ? null : req.Nombre;
                cliente.Contacto = req.Contacto == "" ? null : req.Contacto;
                cliente.Telefono = req.Telefono == "" ? null : req.Telefono;
                cliente.Extension = req.Extension == "" ? null : req.Extension;
                cliente.Rnc = req.Rnc == "" ? null : req.Rnc;
                cliente.Correo = req.Correo == "" ? null : req.Correo;
                cliente.Departamento = req.Departamento == "" ? null : req.Departamento;
                cliente.Direccion = req.Direccion == "" ? null : req.Direccion;
                cliente.IdEmpresa = req.IdEmpresa;
                cliente.Habilitado = req.Habilitado;
                context.Entry(cliente).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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