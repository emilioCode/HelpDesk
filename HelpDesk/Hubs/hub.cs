using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Hubs
{
    public class hub : Hub
    {
        public async Task refresh(string component, int idEmpresa, int? idUsuario, int? idOther)
        {
            await Clients.All.SendAsync("refresh", component, idEmpresa, idUsuario, idOther);
        }
    }
}
