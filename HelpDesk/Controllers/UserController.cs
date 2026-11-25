using HelpDesk.Core.Constants;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Hubs;
using HelpDesk.Responses;
using HelpDesk.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IUserService _userService;
        private readonly IHelpDeskMapper _mapper;
        private readonly IHubContext<HelpDeskHub> _hub;
        private readonly IMemoryCache _cache;
        public UserController(IUserService userService, IHelpDeskMapper mapper, IHubContext<HelpDeskHub> hub, IMemoryCache cache)
        {
            _cache = cache;
            _userService = userService;
            _hub = hub;
            _mapper = mapper;   
        }

        // GET: api/User/1/1
        [HttpGet("{idUser}/{option}")]
        public async Task<IActionResult> Get(int idUser,string option = "unique")
        {
            var users = await _userService.GetUsersByIdAndCondition(idUser, option);
            var userDtos = _mapper.Map(users);
            if (userDtos != null)
            {
                foreach (var userDto in userDtos) userDto.Contrasena = null;
            }
            return Ok(userDtos);
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioDto usuarioDto)
        {
            var user = _mapper.Map(usuarioDto);
            user = await _userService.InsertUSer(user);
            var response = ObjectResponse.Saved(Messages.Saved, WithoutPassword(user));
            return Ok(response);
        }

        // PUT: api/User/5
        [HttpPost("[action]/{idUserReq}")]
        public async Task<IActionResult> Put(int idUserReq, [FromBody] UsuarioDto usuarioDto)
        {
            var user = _mapper.Map(usuarioDto);
            var before = await _userService.GetActiveIdentityAsync(user.Id);
            var result = await _userService.UpdateUSer(user, idUserReq);
            // What the API remembers about this user (their role and company) is no longer true.
            _cache.Remove(AuthOptions.ActiveUserCacheKey(user.Id));
            await NotifyIfAccessChanged(before, user.Id);
            var response = ObjectResponse.Saved(Messages.Saved, result);
            return Ok(response);
        }

        // The role and the company travel in the token, so a user whose role or company just changed has to sign in again.
        // The notice goes to that person wherever they are connected, and carries the company they had (the one their
        // screen knows). If it does not arrive, the API refuses their old token on the next request anyway.
        private async Task NotifyIfAccessChanged(HelpDesk.Core.CustomEntities.UserIdentity before, int userId)
        {
            if (before == null) return;
            var after = await _userService.GetActiveIdentityAsync(userId);
            if (after != null && (after.Acceso != before.Acceso || after.IdEmpresa != before.IdEmpresa))
            {
                await _hub.Clients.Group(HelpDeskHub.GroupOfUser(userId)).SendAsync("refresh", "session", before.IdEmpresa, userId, 2);
            }
        }

        // Passwords, hashed or not, are never sent back to the client.
        private UsuarioDto WithoutPassword(Usuario user)
        {
            var dto = _mapper.Map(user);
            if (dto != null) dto.Contrasena = null;
            return dto;
        }
    }
}
