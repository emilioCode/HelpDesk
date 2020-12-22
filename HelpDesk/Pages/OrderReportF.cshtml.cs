using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;

namespace HelpDesk.Pages
{
    public class OrderReportFModel : PageModel
    {
        public string title { get; set; }
        public Empresa empresa { get; set; }
        public Solicitud solicitud { get; set; }
        public List<Equipo> equipos { get; set; }
        public Cliente cliente { get; set; }
        public List<Seguimiento> seguimiento { get; set; }
        public string soluciones { get; set; }
        public List<Piezas> piezas { get; set; }

        private readonly HelpDeskDBContext context;

        public OrderReportFModel(HelpDeskDBContext _context)
        {
            this.context = _context;
        }


        public void OnGet(string value)
        {
            var noTicket = value.Split('-')[0];
            var idEmpresa = Convert.ToInt32(value.Split('-')[1]);
            empresa = context.Empresa.Find(idEmpresa);
            solicitud = context.Solicitud.Where(e => e.NoSecuencia == noTicket).First();
            cliente = context.Cliente.Where(e => e.IdEmpresa == idEmpresa && e.Id == solicitud.IdCliente).First();
            equipos = context.Equipo.Where(e => e.IdEmpresa == idEmpresa && e.IdSolicitud == solicitud.Id).ToList();
            piezas = context.Piezas.Where(e => e.IdEmpresa == idEmpresa && e.IdSolicitud == solicitud.Id).ToList();

            seguimiento = context.Seguimiento.Where(e => e.IdEmpresa == idEmpresa && e.IdSolicitud == solicitud.Id && e.Etiquetado == true).ToList();
            soluciones = seguimiento.Select(x => x.Texto).Join(",");

            while (equipos.Count < 4)
            {
                var equipoTemp = new Equipo();
                equipos.Add(equipoTemp);
            }
            while (piezas.Count < 4)
            {
                var piezaTemp = new Piezas();
                piezas.Add(piezaTemp);
            }
            title = $"HelpDesk - Service Order No.{noTicket}";

        }
    }
}