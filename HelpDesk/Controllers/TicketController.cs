using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        public TicketController(ITicketService ticketService, IMapper mapper, IMailService mailService)
        {
            _ticketService = ticketService;
            _mapper = mapper;
            _mailService = mailService;
        }
        // GET: api/Ticket/1/all
        [HttpGet("{userId}/{option}")]
        public async Task<IActionResult> Get(int userId, string option = "unique")
        {
            var tickets = await _ticketService.GetTicketsByIdAndCondition(userId, option);
            return Ok(tickets);
        }

        // POST: api/Ticket
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SolicitudDto ticketDto)
        {
            var ticket = _mapper.Map<Solicitud>(ticketDto);
            var result = await _ticketService.CreateTicket(ticket);
            ticket = result.Item1;
            
            if (result.Item2 != null)
            {
                var resp = _mailService.Send(result.Item2.Host, result.Item2.Port, System.Net.Mail.SmtpDeliveryMethod.Network, false,
                true, result.Item2.EmisorEmail, result.Item2.PasswordEmail, result.Item2.ReceptorEmail, result.Item2.Subject, result.Item2.Body, true);
            }
            var res = new ObjectResponse
                {
                code = "1",
                    title = "Saved",
                    icon = "success",
                    message = "created successfully",
                    data = ticket
            };
            return Ok(res);
        }

        // PUT: api/Ticket/ESTADO
        [HttpPost("[action]/{toDo}")]
        public async Task<IActionResult> EDIT(string toDo, [FromBody] SolicitudDto ticketDto)
        {
            var ticket = _mapper.Map<Solicitud>(ticketDto);
            var results = await _ticketService.UpdateTicket(ticket, toDo);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been updated successfully",
                data = results.Item1
            };
            if (results.Item2 != null)
            {
                var resp = _mailService.Send(results.Item2.Host, results.Item2.Port, System.Net.Mail.SmtpDeliveryMethod.Network, false,
                        true, results.Item2.EmisorEmail, results.Item2.PasswordEmail, results.Item2.ReceptorEmail, results.Item2.Subject, results.Item2.Body, true);
            }
            return Ok(response);
        }

        [HttpGet("[action]/{userId}")]
        public async Task<IActionResult> numbersOfTickets(int userId)
        {
            var metrics = await _ticketService.GetMetricsByUserId(userId);
            var response = new ObjectResponse
            {
                code = "1",
                data = metrics
            };
            return Ok(response);
        }

        [HttpPost("[action]")]
        public IActionResult GetJsonTicket([FromBody] SolicitudLiteDto ticketLiteDto)
        {
            var ticket = _mapper.Map<Solicitud>(ticketLiteDto);
            var response = _ticketService.GetSummaryInformation(ticket);
            return Ok(response);
        }
    }
}
