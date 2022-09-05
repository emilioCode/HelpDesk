using HelpDesk.Core.Entities;
using System;

namespace HelpDesk.Core.CustomEntities
{
    public class Event
    {
        public int id { get; set; }
        public string title { get; set; }
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public Solicitud ticket { get; set; }
        public Usuario user { get; set; }
    }
}
