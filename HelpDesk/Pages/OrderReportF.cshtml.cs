using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.Constants;

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

        private readonly IDeviceService _deviceService;
        private readonly IBusinessService _businessService;
        private readonly ICustomerService _customerService;
        private readonly ITicketService _ticketService;
        private readonly IPieceService _pieceService;
        private readonly ITraceService _traceService;

        public OrderReportFModel(IDeviceService deviceService, IBusinessService businessService, ICustomerService customerService, ITicketService ticketService, IPieceService pieceService, ITraceService traceService)
        {
            _deviceService = deviceService;
            _businessService = businessService;
            _customerService = customerService;
            _ticketService = ticketService;
            _pieceService = pieceService;
            _traceService = traceService;
        }

        public async Task<IActionResult> OnGetAsync(string value)
        {
            var parts = value?.Split('-');
            if (parts == null || parts.Length != 2 || !int.TryParse(parts[1], out var idEmpresa)) return NotFound();
            var noTicket = parts[0];
            empresa = await _businessService.GetById(idEmpresa);
            solicitud = _ticketService.GetTicketBySecuencialNumber(noTicket).FirstOrDefault(x => x.IdEmpresa == idEmpresa);
            if (empresa == null || solicitud == null) return NotFound();
            var listOfCustomer = await _customerService.GetCustomersByIdAndCondition(solicitud.IdCliente, "unique");
            cliente = listOfCustomer.ToList().First();
            equipos = _deviceService.GetDevicesByTicketId(solicitud.Id).Where(e => e.IdEmpresa == idEmpresa).ToList();
            piezas = _pieceService.GetPieces(solicitud.Id, idEmpresa).ToList();
            seguimiento = _traceService.GetAllTraces().Where(e => e.IdEmpresa == idEmpresa && e.IdSolicitud == solicitud.Id && e.Etiquetado == true).ToList();
            soluciones = string.Join(',', seguimiento.Select(x => x.Texto));
            while (equipos.Count < 4)
            {
                var equipoTemp = new Equipo();
                equipos.Add(equipoTemp);
            }
            while (piezas.Count < 3)
            {
                var piezaTemp = new Piezas();
                piezas.Add(piezaTemp);
            }
            title = Messages.ServiceOrderTitle(noTicket);
            return Page();
        }
    }
}