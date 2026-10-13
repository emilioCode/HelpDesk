using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Core.Constants;

namespace HelpDesk.Pages
{
    public class OrderReport1Model : PageModel
    {
        public string title { get; set; }
        public Empresa empresa { get; set; }
        public Solicitud solicitud { get; set; }
        public List<Equipo> equipos { get; set; }
        public Cliente cliente { get; set; }

        private readonly ICurrentUser _currentUser;
        private readonly IDeviceService _deviceService;
        private readonly IBusinessService _businessService;
        private readonly ICustomerService _customerService;
        private readonly ITicketService _ticketService;
        public OrderReport1Model(ICurrentUser currentUser, IDeviceService deviceService, IBusinessService businessService, ICustomerService customerService, ITicketService ticketService)
        {
            _currentUser = currentUser;
            _deviceService = deviceService;
            _businessService = businessService;
            _customerService = customerService;
            _ticketService = ticketService;
        }
        public async Task<IActionResult> OnGetAsync(string value)
        {
            var parts = value?.Split('-');
            if (parts == null || parts.Length != 2 || !int.TryParse(parts[1], out var idEmpresa)) return NotFound();
            var noTicket = parts[0];
            if (_currentUser.IsAuthenticated && idEmpresa != _currentUser.CompanyId) return Forbid();
            empresa = await _businessService.GetById(idEmpresa);
            solicitud = _ticketService.GetTicketBySecuencialNumber(noTicket).FirstOrDefault(x => x.IdEmpresa == idEmpresa);
            if (empresa == null || solicitud == null) return NotFound();
            var listOfCustomer = await _customerService.GetCustomersByIdAndCondition(solicitud.IdCliente, "unique");
            cliente = listOfCustomer.ToList().First();
            equipos = _deviceService.GetDevicesByTicketId(solicitud.Id).Where(e => e.IdEmpresa == idEmpresa).ToList();
            while (equipos.Count < 4)
            {
                var equipoTemp = new Equipo();
                equipos.Add(equipoTemp);
            }
            title = Messages.ServiceOrderTitle(noTicket);
            return Page();
        }
    }
}