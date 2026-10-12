using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraceController : ControllerBase
    {
        private readonly ITraceService _traceService;
        private readonly IMapper _mapper;
        public TraceController(ITraceService traceService, IMapper mapper)
        {
            _traceService = traceService;   
            _mapper = mapper;
        }
        // GET: api/Trace/1/1
        [HttpGet("{ticketId}/{businnesId}")]
        public IActionResult Get(int ticketId, int businnesId)
        {
            var customTraces = _traceService.GetTracesById(ticketId, businnesId);
            var response = ObjectResponse.Saved("has been saved successfully", customTraces);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SeguimientoDto traceDto)
        {
            var trace = _mapper.Map<Seguimiento>(traceDto);
            trace = await _traceService.AddTrace(trace);
            traceDto = _mapper.Map<SeguimientoDto>(trace);
            var response = ObjectResponse.Saved("has been saved successfully", traceDto);
            return Ok(response);
        }

        // PUT: api/Trace/Put
        [HttpPost("[action]")]
        public async Task<IActionResult> Put([FromBody] SeguimientoDto traceDto)
        {
            var trace = _mapper.Map<Seguimiento>(traceDto);
            trace = await _traceService.UpdateTrace(trace);
            traceDto = _mapper.Map<SeguimientoDto>(trace);
            var response = ObjectResponse.Saved("has been updated successfully", traceDto);
            return Ok(response);
        }
    }
}
