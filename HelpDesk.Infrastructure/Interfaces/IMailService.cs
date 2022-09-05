using System.Net.Mail;

namespace HelpDesk.Infrastructure.Interfaces
{
    public interface IMailService
    {
        bool Send(string host, int port, SmtpDeliveryMethod method, bool useDefaultCredentials, bool enableSsl, string emailFrom, string password, string emailTo, string subject, string message, bool isBodyHtml);
        bool TestConnection();
    }
}
