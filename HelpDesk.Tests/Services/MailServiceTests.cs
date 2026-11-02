using HelpDesk.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Sockets;

namespace HelpDesk.Tests.Services
{
    public class MailServiceTests
    {
        private class RecordingLogger : ILogger<MailService>
        {
            public List<string> Warnings { get; } = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Warning) { Warnings.Add(formatter(state, exception)); }
            }
        }

        private static int FreePort()
        {
            var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [Test]
        public void Send_WhenTheMailServerCannotBeReached_ReturnsFalseAndLogsTheReason()
        {
            var logger = new RecordingLogger();
            var service = new MailService(logger);

            var sent = service.Send("127.0.0.1", FreePort(), SmtpDeliveryMethod.Network, false, false,
                "from@test.local", "secret", "to@test.local", "Order 7", "<b>hi</b>", true);

            Assert.IsFalse(sent);
            Assert.AreEqual(1, logger.Warnings.Count);
            StringAssert.Contains("Order 7", logger.Warnings[0]);
            StringAssert.DoesNotContain("secret", logger.Warnings[0], "the password is never written to the log");
        }

        [Test]
        public void Send_WithAnInvalidAddress_ReturnsFalseInsteadOfThrowing()
        {
            var service = new MailService(new RecordingLogger());

            var sent = service.Send("127.0.0.1", FreePort(), SmtpDeliveryMethod.Network, false, false,
                "not an address", "x", "to@test.local", "s", "b", false);

            Assert.IsFalse(sent);
        }
    }
}
