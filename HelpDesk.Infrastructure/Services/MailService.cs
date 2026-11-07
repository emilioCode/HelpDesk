using HelpDesk.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;

namespace HelpDesk.Infrastructure.Services
{
    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;

        public MailService(ILogger<MailService> logger = null, IConfiguration configuration = null)
        {
            _logger = logger;
            // Only for a development mail catcher (Mailpit) that presents a self-signed certificate. Off by default.
            if (string.Equals(configuration?["Mail:AllowUntrustedCertificates"], "true", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            }
        }

        public bool Send(string host, int port, SmtpDeliveryMethod method, Boolean useDefaultCredentials, Boolean enableSsl,
    string emailFrom, string password, string emailTo, string subject, string message, Boolean isBodyHtml)
        {
            try
            {
                using (var client = new SmtpClient(host))
                {
                    client.Port = port;
                    client.DeliveryMethod = method;
                    client.UseDefaultCredentials = useDefaultCredentials;
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(emailFrom, password);

                    using (var mail = new MailMessage(emailFrom, emailTo))
                    {
                        mail.Subject = subject;
                        mail.Body = message;
                        mail.IsBodyHtml = isBodyHtml;
                        client.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // The caller carries on (the ticket change is already saved), so the reason is written to the log.
                _logger?.LogWarning(ex, "The e-mail '{Subject}' could not be sent through {Host}:{Port}", subject, host, port);
                return false;
            }
        }

        // Checks that the machine has Internet access. The e-mail is no longer held back by it: an internal mail
        // server works without Internet, and a failed send is logged with its real cause.
        public bool TestConnection()
        {
            try
            {
                var request = WebRequest.Create(new Uri("http://www.google.com/"));
                using (request.GetResponse())
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
