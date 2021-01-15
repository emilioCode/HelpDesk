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
    public class UserController : ControllerBase
    {
        private readonly HelpDeskDBContext context;
        public UserController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        // GET: api/User/1/1
        [HttpGet("{idUser}/{option}")]
        public JsonResult Get(int idUser,string option = "unique")
        {
            List<Usuario> usuarios = new List<Usuario>();
            
            Usuario usuario = context.Usuario.Find(idUser);
            if (option == "JUST NAME") {
                var users = context.Usuario.Where(e => e.IdEmpresa == usuario.IdEmpresa && e.Acceso != "ROOT").Select(e=> new { e.Id, e.Nombre}).ToList();
                return new JsonResult(users.OrderByDescending(x => x.Id));
            }
            else if (option.ToLower() == "unique")
            {
                usuarios.Add(usuario);
            }
            else if(option =="*" || option.ToLower() == "all")
            {
                switch (usuario.Acceso)
                {
                    case "ROOT":
                        usuarios.AddRange(context.Usuario);
                        break;
                    case "MODERADOR":
                        //break;
                    case "ADMINISTRADOR":
                        usuarios.AddRange(context.Usuario.Where(u=>u.Acceso != "ROOT" && u.IdEmpresa==usuario.IdEmpresa));
                        break;

                    case "TECNICO":                              
                        break;
                    default:             
                        break;
                }
            }    
            return new JsonResult(usuarios.OrderByDescending(x => x.Id));
        }

        // GET: api/User/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/User
        [HttpPost]
        public JsonResult Post([FromBody] Usuario req)
        {
            ObjectResponse res;
            try
            {
                if (req.Nombre == null || req.Nombre == "" ||
                    req.CuentaUsuario == null || req.CuentaUsuario == "" ||
                    req.Contrasena == null || req.Contrasena == "" ||
                    req.IdEmpresa == null || req.IdEmpresa <= 0)
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
                int quantity = context.Usuario.Where(e => e.IdEmpresa == req.IdEmpresa).Count();
                var limit = context.Empresa.Where(b => b.Id == req.IdEmpresa).Select(b=>b.Limit).SingleOrDefault();
                limit = limit == null ? 1 : limit;
                if (quantity > limit)
                {
                    res = new ObjectResponse
                    {
                        code = "2",
                        title = "No fue posible agregar usuario",
                        icon = "warning",
                        message = $"Las cuentas de usuario o usuarios que puedes tener como máximo registrados en la plataforma es de {limit}.",
                        data = null
                    };
                    return new JsonResult(res);
                }

                List <Usuario> userAccounts = context.Usuario.Where(uac => uac.IdEmpresa == req.IdEmpresa && uac.Id != req.Id).ToList();

                for (int i = 0; i < userAccounts.Count; i++)
                {
                    if (userAccounts[i].CuentaUsuario.Equals(req.CuentaUsuario))
                    {
                        res = new ObjectResponse
                        {
                            code = "2",
                            title = "Validation errors",
                            icon = "warning",
                            message = $"the userAccount <b>{ req.CuentaUsuario }</b> already exists",
                            data = req
                        };
                        return new JsonResult(res);
                    }
                }
                req.Habilitado = true;
                context.Entry(req).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                context.SaveChanges();

                var data = context.Usuario.Where(e => e.Nombre == req.Nombre
                && e.NumDocumento == req.NumDocumento && e.CuentaUsuario == req.CuentaUsuario && e.Acceso == req.Acceso
                && e.Correo == req.Correo && e.Contrasena == req.Contrasena).SingleOrDefault();
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

        // PUT: api/User/5
        //[HttpPut("{idUserReq}")]
        [HttpPost("[action]/{idUserReq}")]
        public JsonResult Put(int idUserReq, [FromBody] Usuario req)
        {
            ObjectResponse res;
            try
            {
                if (req.Nombre == null || req.Nombre == "" ||
                    req.CuentaUsuario == null || req.CuentaUsuario == "" ||
                    req.Contrasena == null || req.Contrasena =="" ||
                    req.IdEmpresa==null || req.IdEmpresa <= 0)
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

                List<Usuario> userAccounts = context.Usuario.Where(uac => uac.IdEmpresa == req.IdEmpresa && uac.Id != req.Id).ToList();

                for (int i = 0; i < userAccounts.Count; i++)
                {
                    if (userAccounts[i].CuentaUsuario.Equals(req.CuentaUsuario))
                    {
                        res = new ObjectResponse
                        {
                            code = "2",
                            title = "Validation errors",
                            icon = "warning",
                            message = $"the userAccount <b>{ req.CuentaUsuario }</b> already exists",
                            data = req
                        };
                        return new JsonResult(res);
                    }
                }
                var userLevel = context.Usuario.Find(idUserReq);
                var usuario = context.Usuario.Find(req.Id);
                usuario.Nombre = req.Nombre==""?null:req.Nombre;
                usuario.NumDocumento = req.NumDocumento == "" ? null : req.NumDocumento;
                if(userLevel.Acceso == "ROOT" && req.CuentaUsuario != null && req.CuentaUsuario != "")
                    usuario.CuentaUsuario = req.CuentaUsuario;
                usuario.Contrasena = req.Contrasena == "" ? null : req.Contrasena;  
                usuario.Acceso = req.Acceso == "" ? null : req.Acceso;
                usuario.Correo = req.Correo == "" ? null : req.Correo;
                usuario.IdEmpresa = req.IdEmpresa;
                usuario.Image = req.Image;
                usuario.Habilitado = req.Habilitado;
                context.Entry(usuario).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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

        // PACTH: api/ApiWithActions/5
        [HttpPatch("{password}")]
        public string Patch(string password)
        {
            return Security.Encripting(password);
        }
    }
}
