using HelpDesk.Infrastructure.Interfaces;
using System;
using System.Net.Mail;

namespace HelpDesk.Infrastructure.Services
{
    public class MailService : IMailService
    {
        public bool Send(string host, int port, SmtpDeliveryMethod method, Boolean useDefaultCredentials, Boolean enableSsl,
    string emailFrom, string password, string emailTo, string subject, string message, Boolean isBodyHtml)
        {
            bool response = true;
            try
            {
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
                }
                else
                {
                    response = false;
                }

            }
            catch (Exception ex)
            {
                response = false;
            }
            return response;
        }

        #region TestConnection
        /// <summary>
        /// test Internet return Boolean
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            bool result = false;
            System.Uri Url = new System.Uri("http://www.google.com/");
            System.Net.WebRequest WebRequest;
            WebRequest = System.Net.WebRequest.Create(Url);
            System.Net.WebResponse objResp;
            try
            {
                objResp = WebRequest.GetResponse();
                result = true;
                objResp.Close();
                WebRequest = null;
            }
            catch
            {
                result = false;
                WebRequest = null;
            }
            return result;
        }
        #endregion TestConnection
    }
}
