using System.Collections.Generic;
using HelpDesk.Core.Constants;

namespace HelpDesk.Core.Dictionaries
{
    public class Levels
    {
        public static Dictionary<string, int> AccessLevel = new Dictionary<string, int>()
        {
            { Roles.Root, 4 },
            { Roles.Admin, 3 },
            { Roles.Moderator, 2 },
            { Roles.Technician, 1 }
        };
    }
}