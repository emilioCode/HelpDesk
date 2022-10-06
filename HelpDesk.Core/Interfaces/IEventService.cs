using HelpDesk.Core.CustomEntities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDesk.Core.Interfaces
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetEvent(int userId);
    }
}
