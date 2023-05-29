using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelpDesk.Pages
{
    public class OrderReport1Model : PageModel
    {
        public string title { get; set; }
        public Empresa empresa { get; set; }
        public Solicitud solicitud { get; set; }
        public List<Equipo> equipos { get; set; }
        public Cliente cliente { get; set; }

        private readonly IDeviceService _deviceService;
        private readonly IBusinessService _businessService;
        private readonly ICustomerService _customerService;
        private readonly ITicketService _ticketService;
        public OrderReport1Model(IDeviceService deviceService, IBusinessService businessService, ICustomerService customerService, ITicketService ticketService)
        {
            _deviceService = deviceService;
            _businessService = businessService;
            _customerService = customerService;
            _ticketService = ticketService;
        }
        public async void OnGet(string value)
        {
            var noTicket = value.Split('-')[0];
            var idEmpresa = Convert.ToInt32(value.Split('-')[1]);
            empresa = await _businessService.GetById(idEmpresa);
            solicitud = _ticketService.GetTicketBySecuencialNumber(noTicket).First();
            var listOfCustomer = await _customerService.GetCustomersByIdAndCondition(solicitud.IdCliente, "unique");
            cliente = listOfCustomer.ToList().First();
            equipos = _deviceService.GetDevicesByTicketId(solicitud.IdCliente).ToList();
            while (equipos.Count < 4)
            {
                var equipoTemp = new Equipo();
                equipos.Add(equipoTemp);
            }
            title = $"HelpDesk - Service Order No.{noTicket}";
        }
    }
}