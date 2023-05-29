using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventController(HelpDeskDBContext _context, IEventService eventService)
        {
            _eventService = eventService;
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            var data = await _eventService.GetEvent(usuario.Id);
            var res = new ObjectResponse
            {
                data = data,
            };
            return Ok(res);
        }
    }
}
