using HelpDesk.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.Pages
{
    public class EncryptModel : PageModel
    {
        private readonly ISecurityService _securityService;
        public EncryptModel(ISecurityService securityService)
        {
            _securityService = securityService;
        }
        public string request { get; set; }
        public string result { get; set; }
        public void OnGet()
        {
            request = "";
        }

        public void OnPost(string pwd)
        {
            result = _securityService.Encripting(pwd);
        }
    }
}