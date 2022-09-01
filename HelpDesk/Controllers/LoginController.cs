using AutoMapper;
using HelpDesk.Core.CustomEntitties;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public LoginController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        // GET: api/Login/1/user01
        [HttpGet("{idEmpresa}/{userAccount}")]
        public async Task<IActionResult> Get(int idEmpresa, string userAccount)
        {
            var result = await _userService.ValidateUserAccount(idEmpresa, userAccount);
            return Ok(result);
        }

        // POST: api/Login
        [HttpPost]
        public IActionResult Post([FromBody] UserLogin loginUser)
        {
            var user = _mapper.Map<Usuario>(loginUser);
            var userIdentity = _userService.GetLoginByCredentials(user);
            return Ok(userIdentity);
        }
    }
}
