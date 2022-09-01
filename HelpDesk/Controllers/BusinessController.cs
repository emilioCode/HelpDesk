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
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IMapper _mapper;
        public BusinessController(IBusinessService businessService, IMapper mapper)
        {
            _businessService = businessService;
            _mapper = mapper;
        }

        // GET: api/Business/1
        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            var businesses = await _businessService.GetBusinessesByUserAccess(userId);
            var businessDtos = _mapper.Map<IEnumerable<EmpresaDto>>(businesses);
            return Ok(businessDtos);
        }

        // POST: api/Business/1
        [HttpPost("{userId}")]
        public async Task<IActionResult> Post(int userId, [FromBody] EmpresaDto businessDto)
        {
            var business = _mapper.Map<Empresa>(businessDto);
            business = await _businessService.CreateBusinness(userId, business);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = business
            };
            return Ok(response);
        }

        // PUT: api/Business/Put/5
        //[HttpPut("{id}")]
        [HttpPost("[action]/{userId}")]
        public async Task<IActionResult> Put(int userId, [FromBody] EmpresaDto businessDto)
        {
            var business = _mapper.Map<Empresa>(businessDto);
            var result = await _businessService.UpdateBusiness(userId, business);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been updated successfully",
                data = result
            };
            return Ok(response);
        }
    }
}
