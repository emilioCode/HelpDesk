using HelpDesk.Security;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelpDesk.Hubs
{
    public class hub : Hub
    {
        public static string GroupOf(int idEmpresa) => "empresa-" + idEmpresa;

        // Notices for one person, wherever they are connected from. Only the server sends to it.
        public static string GroupOfUser(int idUsuario) => "usuario-" + idUsuario;

        // Every connection joins the group of its own company and the one of its own user, taken from the token and never from what the browser says.
        public override async Task OnConnectedAsync()
        {
            var company = CompanyOf(Context.User);
            if (company > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupOf(company));
            }
            if (int.TryParse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupOfUser(userId));
            }
            await base.OnConnectedAsync();
        }

        // The notice only reaches the sender's company. The company in the call is replaced by the one in the token,
        // so a browser cannot notify another company. Without a company in the token nobody is notified.
        // The ids come from the screens as they are in a form, so a number may arrive as text ("2", from a selector).
        // They are accepted either way; a value that is not a number is delivered as nothing.
        public async Task refresh(string component, object idEmpresa, object idUsuario, object idOther)
        {
            var company = CompanyOf(Context.User);
            if (company <= 0)
            {
                return;
            }
            await Clients.Group(GroupOf(company)).SendAsync("refresh", component, company, AsInt(idUsuario), AsInt(idOther));
        }

        public static int? AsInt(object value)
        {
            switch (value)
            {
                case null: return null;
                case int number: return number;
                case long big when big >= int.MinValue && big <= int.MaxValue: return (int)big;
                case string text: return int.TryParse(text, out var parsed) ? parsed : (int?)null;
                case JsonElement json when json.ValueKind == JsonValueKind.Number: return json.TryGetInt32(out var n) ? n : (int?)null;
                case JsonElement json when json.ValueKind == JsonValueKind.String: return int.TryParse(json.GetString(), out var s) ? s : (int?)null;
                default: return null;
            }
        }

        private static int CompanyOf(ClaimsPrincipal user)
        {
            return int.TryParse(user?.FindFirst(JwtTokenService.CompanyClaim)?.Value, out var company) ? company : 0;
        }
    }
}
