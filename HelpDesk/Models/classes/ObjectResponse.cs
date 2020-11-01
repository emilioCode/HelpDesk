using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.classes
{
    public class ObjectResponse
    {
        public string title { get; set; }
        public string message { get; set; }
        public string icon { get; set; }
        public string code { get; set; }
        public object data { get; set; }
    }
}
