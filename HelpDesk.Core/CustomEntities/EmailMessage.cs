using System;
using System.Collections.Generic;
using System.Text;

namespace HelpDesk.Core.CustomEntities
{
    public class EmailMessage
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ReceptorEmail { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string EmisorEmail { get; set; }
        public string PasswordEmail { get; set; }

    }
}
