using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Interfaces;
using HelpDesk.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISecurityService _securityService;
        public UserController(IUserService userService, IMapper mapper, ISecurityService securityService)
        {
            _userService = userService;
            _mapper = mapper;   
            _securityService = securityService;
        }

        // GET: api/User/1/1
        [HttpGet("{idUser}/{option}")]
        public async Task<IActionResult> Get(int idUser,string option = "unique")
        {
            var users = await _userService.GetUsersByIdAndCondition(idUser, option);
            var userDtos = _mapper.Map<IEnumerable<UsuarioDto>>(users);
            return Ok(userDtos);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioDto usuarioDto)
        {
            var user = _mapper.Map<Usuario>(usuarioDto);
            user = await _userService.InsertUSer(user);
            var response = new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = "has been saved successfully",
                data = user
            };
            return Ok(response);
        }

        // PUT: api/User/5
        [HttpPost("[action]/{idUserReq}")]
        public async Task<IActionResult> Put(int idUserReq, [FromBody] UsuarioDto usuarioDto)
        {
            var user = _mapper.Map<Usuario>(usuarioDto);
            var result = await _userService.UpdateUSer(user, idUserReq);
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

        // PACTH: api/ApiWithActions/5
        [HttpPatch("{password}")]
        public string Patch(string password)
        {
            return _securityService.Encripting(password);
        }
    }
}
