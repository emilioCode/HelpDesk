using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.Pages
{
    public class EncryptModel : PageModel
    {
        public string request { get; set; }
        public string result { get; set; }
        public void OnGet()
        {
            request = "";
        }

        public void OnPost(string pwd)
        {
            result = pwd != "" ? Security.Encripting(pwd) : "";
        }
    }
}