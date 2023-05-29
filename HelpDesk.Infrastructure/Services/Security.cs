using HelpDesk.Infrastructure.Interfaces;
using System;

namespace HelpDesk.Infrastructure.Services
{
    public class Security : ISecurityService
    {
        public string Encripting(string password)
        {
            string result = string.Empty;
            try
            {
                byte[] encryted = System.Text.Encoding.Unicode.GetBytes(password);
                result = Convert.ToBase64String(encryted);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public string Decrypting(string password)
        {
            string result = string.Empty;
            try
            {
                byte[] decryted = Convert.FromBase64String(password);
                result = System.Text.Encoding.Unicode.GetString(decryted);
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
