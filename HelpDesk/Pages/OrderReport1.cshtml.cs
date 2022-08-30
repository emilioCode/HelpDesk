using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.Entities;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.Pages
{
    public class OrderReport1Model : PageModel
    {
        public string title { get; set; }
        public Empresa empresa { get; set; }
        public Solicitud solicitud { get; set; }
        public List<Equipo> equipos { get; set; }
        public Cliente cliente { get; set; }

        private readonly HelpDeskDBContext context;

        public OrderReport1Model(HelpDeskDBContext _context)
        {
            this.context = _context;
        }
        public void OnGet(string value)
        {
            var noTicket = value.Split('-')[0];
            var idEmpresa = Convert.ToInt32(value.Split('-')[1]);
            empresa = context.Empresas.Find(idEmpresa);
            solicitud = context.Solicitudes.Where(e => e.NoSecuencia == noTicket).First();
            cliente = context.Clientes.Where(e => e.IdEmpresa == idEmpresa && e.Id == solicitud.IdCliente).First();
            equipos = context.Equipos.Where(e => e.IdEmpresa == idEmpresa && e.IdSolicitud == solicitud.Id).ToList();
            while (equipos.Count < 4)
            {
                var equipoTemp = new Equipo();
                equipos.Add(equipoTemp);
            }
            title = $"HelpDesk - Service Order No.{noTicket}";
        }
    }
}