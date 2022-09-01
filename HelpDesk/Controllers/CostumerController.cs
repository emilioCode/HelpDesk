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
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class CostumerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        public CostumerController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        // GET: api/Costumer/1/all
        [HttpGet("{idUserOridClient}/{option}")]
        public async Task<IActionResult> Get(int idUserOridClient, string option = null)
        {
            var customers = await _customerService.GetCustomersByIdAndCondition(idUserOridClient, option);
            var customerDtos = _mapper.Map<IEnumerable<ClienteDto>>(customers);
            return Ok(customerDtos);
        }

        // POST: api/Costumer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClienteDto customerDto)
        {
            var customer = _mapper.Map<Cliente>(customerDto);
            customer = await _customerService.CreateCustomer(customer);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = customer
            };
            return Ok(response);
        }

        // PUT: api/Costumer/Put/5
        [HttpPost("[action]/{userId}")]
        public async Task<IActionResult> Put(int userId, [FromBody] ClienteDto customerDto)
        {
            var customer = _mapper.Map<Cliente>(customerDto);
            var result = await _customerService.UpdateCustomer(customer, userId);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = result
            };
            return Ok(response);
        }
    }
}
