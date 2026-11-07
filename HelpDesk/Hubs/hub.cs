using HelpDesk.Security;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelpDesk.Hubs
{
    public class hub : Hub
    {
        public static string GroupOf(int idEmpresa) => "empresa-" + idEmpresa;

        // Every connection joins the group of its own company, taken from the token and never from what the browser says.
        public override async Task OnConnectedAsync()
        {
            var company = CompanyOf(Context.User);
            if (company > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupOf(company));
            }
            await base.OnConnectedAsync();
        }

        // The notice only reaches the sender's company. The company in the call is replaced by the one in the token,
        // so a browser cannot notify another company. Without a company in the token nobody is notified.
        public async Task refresh(string component, int idEmpresa, int? idUsuario, int? idOther)
        {
            var company = CompanyOf(Context.User);
            if (company <= 0)
            {
                return;
            }
            await Clients.Group(GroupOf(company)).SendAsync("refresh", component, company, idUsuario, idOther);
        }

        private static int CompanyOf(ClaimsPrincipal user)
        {
            return int.TryParse(user?.FindFirst(JwtTokenService.CompanyClaim)?.Value, out var company) ? company : 0;
        }
    }
}
