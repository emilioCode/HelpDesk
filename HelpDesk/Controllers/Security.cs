using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    public class Security
    {
        public static string Encripting(string password)
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

        public static string Decrypting(string password)
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
