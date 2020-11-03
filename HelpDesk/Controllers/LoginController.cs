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
    public class LoginController : ControllerBase
    {
        private readonly HelpDeskDBContext context;

        public LoginController(HelpDeskDBContext _context)
        {
            this.context = _context;
        }

        // GET: api/Login/1/user01
        [HttpGet("{idEmpresa}/{userAccount}")]
        public JsonResult Get(int idEmpresa, string userAccount)
        {
            List<String> userAccounts = context.Usuario
                .Where(u => u.IdEmpresa == idEmpresa).Select(u=>u.CuentaUsuario).ToList();
            ObjectResponse res = userAccounts.Where(uac => uac == userAccount).Count() > 0 ? 
                new ObjectResponse
                {
                    code = "1",
                    title = "Validations",
                    icon = "success",
                    message = "It's ok.",
                    data = false
                } : 
                new ObjectResponse
                {
                    code = "1",
                    title = "Validations",
                    icon = "warning",
                    message = "There exists another account with this name",
                    data = true
                };
            
            return new JsonResult(res);
        }

        // GET: api/Login/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Login
        [HttpPost]
        public JsonResult Post([FromBody] Usuario loginUser)
        {
            var user = from usuario in context.Usuario
                       join empresa in context.Empresa on usuario.IdEmpresa equals empresa.Id
                       where usuario.CuentaUsuario == loginUser.CuentaUsuario
                       //&& (checkHash(loginUser.pwd, usuario.Contrasena, hashType.MD5) == true)
                       && usuario.Contrasena == loginUser.Contrasena
                       && empresa.Id == usuario.IdEmpresa && usuario.Habilitado == true && ( empresa.Habilitado == true || usuario.Acceso=="ROOT")
                       select new
                       {
                           id = usuario.Id,
                           nombre = usuario.Nombre,
                           acceso = usuario.Acceso,
                           idEmpresa = usuario.IdEmpresa,
                           empresa = empresa.RazonSocial,
                           userName = usuario.CuentaUsuario,
                           image = usuario.Image,
                           logo = empresa.Image
                       };

            object userFinal = null;
            var users = user.ToList();
            users.ForEach(e =>
            {
                if ((e.userName.Equals(loginUser.CuentaUsuario))) userFinal = e;
            });

            return new JsonResult(userFinal);
        }

        // PUT: api/Login/5
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
