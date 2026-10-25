using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public LoginController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        // GET: api/Login/1/user01
        [HttpGet("{idEmpresa}/{userAccount}")]
        public async Task<IActionResult> Get(int idEmpresa, string userAccount)
        {
            var result = await _userService.ValidateUserAccount(idEmpresa, userAccount);
            return Ok(result);
        }

        // POST: api/Login
        // Returns the user with an access token, or no content when the credentials are not valid.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] UserLogin loginUser)
        {
            var userIdentity = await _userService.LoginAsync(loginUser.CuentaUsuario, loginUser.Contrasena);
            if (userIdentity == null)
            {
                return Ok(null);
            }

            var token = _tokenService.Create(userIdentity);
            userIdentity.Token = token.Token;
            userIdentity.ExpiresAt = token.ExpiresAt;

            // The printable reports open by a plain link, so they read the same token from this cookie.
            Response.Cookies.Append(AuthOptions.ReportCookie, token.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = Request.IsHttps,
                Expires = token.ExpiresAt,
                Path = "/"
            });
            return Ok(userIdentity);
        }

        // POST: api/Login/Logout
        [HttpPost("[action]")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthOptions.ReportCookie, new CookieOptions { Path = "/" });
            return Ok();
        }
    }
}
