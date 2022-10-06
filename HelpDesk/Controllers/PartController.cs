using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartController : ControllerBase
    {
        private readonly IPieceService _pieceService;
        private readonly IMapper _mapper;
        public PartController(IPieceService pieceService, IMapper mapper)
        {
            _pieceService = pieceService;
            _mapper = mapper;
        }

        // GET: api/Part/1/1
        [HttpGet("{ticketId}/{businessId}")]
        public IActionResult Get(int ticketId, int businessId)
        {
            var parts = _pieceService.GetPieces(ticketId, businessId);
            var partsDtos = _mapper.Map<IEnumerable<PiezasDto>>(parts);
            return Ok(partsDtos);
        }

        // POST: api/Part
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PiezasDto pieceDto)
        {
            var piece = _mapper.Map<Piezas>(pieceDto);
            piece = await _pieceService.AddPiece(piece);
            pieceDto = _mapper.Map<PiezasDto>(piece);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "created successfully",
                data = pieceDto
            };
            return Ok(response);
        }

        // PUT: api/Part
        //[HttpPut]
        [HttpPost("[action]")]
        public async Task<IActionResult> Put([FromBody] PiezasDto pieceDto)
        {
            var piece = _mapper.Map<Piezas>(pieceDto);
            piece = await _pieceService.UpdatePiece(piece);
            pieceDto = _mapper.Map<PiezasDto>(piece);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "modified successfully",
                data = pieceDto
            };
            return Ok(response);
        }

        // DELETE: api/Part/Delete/5
        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _pieceService.DeletePiece(id);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "modified successfully",
                data = result
            };
            return Ok(response);
        }
    }
}
