using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            return new JsonResult(empresas);
        }

        // GET: api/Business/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Business
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Business/5
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
