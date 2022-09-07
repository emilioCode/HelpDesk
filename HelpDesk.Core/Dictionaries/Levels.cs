using System.Collections.Generic;

namespace HelpDesk.Core.Dictionaries
{
    public class Levels
    {
        public static Dictionary<string, int> AccessLevel = new Dictionary<string, int>()
        {
            { "ROOT", 4 },
            { "ADMINISTRADOR", 3 },
            { "MODERADOR", 2 },
            { "TECNICO", 1 }
        };
    }
}