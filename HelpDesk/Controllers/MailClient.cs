using HelpDesk.Models.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HelpDesk.Controllers
{
    public class MailClient
    {
        public static ObjectResponse Send(string host, int port, SmtpDeliveryMethod method, Boolean useDefaultCredentials, Boolean enableSsl,
            string emailFrom, string password, string emailTo, string subject, string message, Boolean isBodyHtml)
        {
            ObjectResponse response;
            try
            {
                response = new ObjectResponse();
                if (TestConnection())
                {
                    SmtpClient client = new SmtpClient(host);
                    client.Port = port;
                    client.DeliveryMethod = method;
                    client.UseDefaultCredentials = useDefaultCredentials;
                    System.Net.NetworkCredential credentials
                        = new System.Net.NetworkCredential(emailFrom, password);

                    client.EnableSsl = enableSsl;
                    client.Credentials = credentials;

                    var mail = new MailMessage(emailFrom, emailTo);
                    mail.Subject = subject;
                    mail.Body = message;
                    mail.IsBodyHtml = isBodyHtml;
                    client.Send(mail);

                    //res = $"success@DONE@The E-mail has been sent succesfully.";
                    response = new ObjectResponse
                    {
                        title = "success",
                        code = "1",
                        message = "The E-mail has been sent succesfully.",
                        data = null
                    };
                }
                else
                {
                    response = new ObjectResponse
                    {
                        title = "ERROR CONNECTION",
                        code = "",
                        message = "Check the internet connection",
                        data = null
                    };
                }

            }
            catch (Exception ex)
            {
                response = new ObjectResponse {
                    title = "ERROR",
                    code = "0",
                    message = ex.Message,
                    data = ex
                };
            }
            return response;
        }

        #region TestConnection
        /// <summary>
        /// test Internet return Boolean
        /// </summary>
        /// <returns></returns>
        public static Boolean TestConnection()
        {
            bool result = false;
            System.Uri Url = new System.Uri("http://www.google.com/");
            System.Net.WebRequest WebRequest;
            WebRequest = System.Net.WebRequest.Create(Url);
            System.Net.WebResponse objResp;
            try
            {
                objResp = WebRequest.GetResponse();
                result = true;//"Su dispositivo está correctamente conectado a internet";
                objResp.Close();
                WebRequest = null;
            }
            catch (Exception /*ex*/)
            {
                result = false;//"Error al intentar conectarse a internet " + ex.Message;
                WebRequest = null;
            }
            return result;
        }
        #endregion TestConnection
    }
}
