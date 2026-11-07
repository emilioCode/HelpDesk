using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    // The real-time notices must stay inside the company that sent them.
    public class RealtimeCompanyTests
    {
        private ApiFactory _factory;
        private readonly List<HubConnection> _connections = new List<HubConnection>();

        [SetUp]
        public void SetUp()
        {
            _factory = new ApiFactory();
            _factory.Seed();
        }

        [TearDown]
        public async Task TearDown()
        {
            foreach (var connection in _connections)
            {
                await connection.DisposeAsync();
            }
            _factory.Dispose();
        }

        // A connected user and every notice they receive, as (component, idEmpresa, idUsuario, idOther).
        private async Task<(HubConnection Connection, ConcurrentQueue<(string, int, int?, int?)> Received)> ConnectAsync(string account)
        {
            var token = await _factory.LoginAsync(account);
            var received = new ConcurrentQueue<(string, int, int?, int?)>();
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, "hub"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                    options.AccessTokenProvider = () => Task.FromResult(token);
                })
                .Build();
            connection.On<string, int, int?, int?>("refresh", (component, company, user, other) => received.Enqueue((component, company, user, other)));
            await connection.StartAsync();
            _connections.Add(connection);
            return (connection, received);
        }

        private static async Task WaitFor(Func<bool> condition)
        {
            for (var i = 0; i < 50 && !condition(); i++)
            {
                await Task.Delay(100);
            }
        }

        // Long enough for a wrongly delivered notice to show up.
        private static Task Settle() => Task.Delay(800);

        [Test]
        public async Task ANotice_ReachesEveryoneInTheSameCompany()
        {
            var sender = await ConnectAsync("tech1");
            var colleague = await ConnectAsync("admin");

            await sender.Connection.InvokeAsync("refresh", "ticket", 1, 4, 1);

            await WaitFor(() => colleague.Received.Count > 0 && sender.Received.Count > 0);
            Assert.That(colleague.Received.ToArray(), Is.EqualTo(new[] { ("ticket", 1, (int?)4, (int?)1) }));
            Assert.That(sender.Received.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task ANotice_NeverReachesAnotherCompany()
        {
            var sender = await ConnectAsync("tech1");
            var otherB = await ConnectAsync("adminb");
            var otherC = await ConnectAsync("adminc");

            await sender.Connection.InvokeAsync("refresh", "ticket", 1, 4, 1);
            await sender.Connection.InvokeAsync("refresh", "session", 1, 4, 1);

            await Settle();
            Assert.That(otherB.Received, Is.Empty);
            Assert.That(otherC.Received, Is.Empty);
        }

        [Test]
        public async Task ANoticeAddressedToAnotherCompany_StaysInTheSendersCompany()
        {
            var sender = await ConnectAsync("tech1");
            var colleague = await ConnectAsync("admin");
            var victim = await ConnectAsync("adminb");

            // The browser claims the notice is for company 2.
            await sender.Connection.InvokeAsync("refresh", "session", 2, 6, 1);

            await WaitFor(() => colleague.Received.Count > 0);
            await Settle();
            Assert.That(victim.Received, Is.Empty, "the other company must not be notified");
            Assert.That(colleague.Received.Single().Item2, Is.EqualTo(1), "the notice carries the company of the token");
        }

        [Test]
        public async Task EachCompany_OnlyHearsItself()
        {
            var a = await ConnectAsync("tech1");
            var b = await ConnectAsync("adminb");
            var c = await ConnectAsync("adminc");

            await a.Connection.InvokeAsync("refresh", "ticket", 1, 4, 1);
            await b.Connection.InvokeAsync("refresh", "costumer", 2, 0, 0);
            await c.Connection.InvokeAsync("refresh", "users", 3, 11, 0);

            await WaitFor(() => a.Received.Count > 0 && b.Received.Count > 0 && c.Received.Count > 0);
            await Settle();
            Assert.That(a.Received.Select(r => r.Item1).ToArray(), Is.EqualTo(new[] { "ticket" }));
            Assert.That(b.Received.Select(r => r.Item1).ToArray(), Is.EqualTo(new[] { "costumer" }));
            Assert.That(c.Received.Select(r => r.Item1).ToArray(), Is.EqualTo(new[] { "users" }));
        }

        [Test]
        public async Task TheHub_RefusesAConnectionWithoutAToken()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(new Uri(_factory.Server.BaseAddress, "hub"), options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                    options.Transports = HttpTransportType.LongPolling;
                })
                .Build();
            _connections.Add(connection);

            Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(() => connection.StartAsync());
        }
    }
}
