using HelpDesk.Core.Entities;
using HelpDesk.Core.Interfaces;
using HelpDesk.Infrastructure.Data;
using HelpDesk.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HelpDesk.Tests.Integration
{
    // The real application on an in-memory database, with two companies and one user of each role.
    public class ApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtKey = "integration-test-jwt-key-with-more-than-32-chars";
        public const string SecretsKey = "integration-test-secrets-key-with-more-than-32-chars";

        private readonly bool? _enforce;
        private readonly int _activeCheckSeconds;

        // Every e-mail the application tried to send.
        public FakeMailService Mail { get; } = new FakeMailService();
        private readonly string _databaseName = Guid.NewGuid().ToString();

        // enforce = null leaves Auth:Enforce unset, to check the default.
        public ApiFactory(bool? enforce = true, int activeCheckSeconds = 0)
        {
            _enforce = enforce;
            _activeCheckSeconds = activeCheckSeconds;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // The single page app is not built in tests; a few stand-in files let the static file rules be checked.
                var dist = Path.Combine(context.HostingEnvironment.ContentRootPath, "ClientApp", "dist");
                Directory.CreateDirectory(dist);
                File.WriteAllText(Path.Combine(dist, "index.html"), "<html><body>HelpDesk</body></html>");
                File.WriteAllText(Path.Combine(dist, "runtime.js"), "// unhashed bundle");
                File.WriteAllText(Path.Combine(dist, "main.0123456789abcdef0123.js"), "// hashed bundle");
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = JwtKey,
                    ["Secrets:Key"] = SecretsKey,
                    ["ConnectionStrings:HelpDesk"] = "unused",
                    ["Jwt:ActiveCheckSeconds"] = _activeCheckSeconds.ToString()
                });
                if (_enforce.HasValue)
                {
                    config.AddInMemoryCollection(new Dictionary<string, string> { ["Auth:Enforce"] = _enforce.Value ? "true" : "false" });
                }
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<HelpDeskDBContext>>();
                services.RemoveAll<HelpDeskDBContext>();
                services.AddDbContext<HelpDeskDBContext>(options => options.UseInMemoryDatabase(_databaseName));
                services.RemoveAll<IMailService>();
                services.AddSingleton<IMailService>(Mail);
            });
        }

        public void Seed()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HelpDeskDBContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var hash = hasher.Hash(TestUsers.Password);

            db.Empresas.AddRange(
                new Empresa { Id = 1, RazonSocial = "Company A", Image = new byte[] { 1, 2, 3 }, Direccion = "Main street 1", Telefono = "555-0100", Correo = "a@test.local", Host = "smtp.test", Port = 25, Contrasena = "mail-secret-a", Secuenciaticket = "3", Limit = 20, Habilitado = true },
                new Empresa { Id = 2, RazonSocial = "Company B", Correo = "b@test.local", Host = "smtp.test", Port = 25, Contrasena = "mail-secret-b", Secuenciaticket = "2", Limit = 20, Habilitado = true },
                new Empresa { Id = 3, RazonSocial = "Company C", Correo = "c@test.local", Host = "smtp.test", Port = 25, Contrasena = "mail-secret-c", Secuenciaticket = "2", Limit = 20, Habilitado = true });
            db.Usuarios.AddRange(
                User(1, "root", "ROOT", 1, "root"),                       // legacy plain-text password
                User(2, "admin", "ADMINISTRADOR", 1, hash),
                User(3, "mod", "MODERADOR", 1, hash),
                User(4, "tech1", "TECNICO", 1, hash),
                User(5, "tech2", "TECNICO", 1, hash),
                User(6, "adminb", "ADMINISTRADOR", 2, hash),
                User(7, "legacy", "TECNICO", 1, "legacy123"),            // legacy plain-text password
                User(8, "lockme", "TECNICO", 1, hash),
                User(9, "tech3", "TECNICO", 1, hash),
                User(10, "pwuser", "TECNICO", 1, hash),                  // changes their password
                User(11, "adminc", "ADMINISTRADOR", 3, hash),
                User(12, "renewrole", "TECNICO", 1, hash),                // gets promoted after signing in
                User(13, "renewoff", "TECNICO", 1, hash),                // gets disabled after signing in
                User(14, "revokeoff", "TECNICO", 1, hash),               // gets disabled while holding a valid token
                User(15, "revokedel", "TECNICO", 1, hash),               // gets deleted while holding a valid token
                User(16, "revokecache", "TECNICO", 1, hash));            // gets disabled inside the cache window

            db.Clientes.AddRange(
                new Cliente { Id = 1, Nombre = "Customer A", Correo = "ca@test.local", IdEmpresa = 1, Habilitado = true },
                new Cliente { Id = 2, Nombre = "Customer B", Correo = "cb@test.local", IdEmpresa = 2, Habilitado = true },
                new Cliente { Id = 3, Nombre = "Customer A2", Correo = "ca2@test.local", IdEmpresa = 1, Habilitado = true });

            var today = DateTime.Now.Date;
            db.Solicitudes.AddRange(
                Ticket(1, "1", 1, 4, 1, today),
                Ticket(2, "2", 1, 5, 1, today),
                Ticket(3, "1", 2, 6, 2, today),
                Ticket(4, "3", 1, 4, 1, today),
                Ticket(5, "4", 1, 4, 1, today),
                Ticket(10, "10", 1, 4, 1, today),   // each of these is changed by one test only
                Ticket(11, "11", 1, 4, 1, today),
                Ticket(12, "12", 1, 4, 1, today),
                Ticket(13, "13", 1, 4, 1, today));

            db.Equipos.AddRange(
                new Equipo { Id = 1, IdSolicitud = 1, IdEmpresa = 1, Descripcion = "Laptop", FallaReportada = "No enciende", Marca = "X", Modelo = "Y", NoSerial = "A1", Habilitado = true },
                new Equipo { Id = 2, IdSolicitud = 3, IdEmpresa = 2, Descripcion = "Printer", FallaReportada = "Atasco", Marca = "X", Modelo = "Y", NoSerial = "B1", Habilitado = true });
            db.Piezas.AddRange(
                new Piezas { Id = 1, IdSolicitud = 1, IdEmpresa = 1, Cantidad = 1, NoSerial = "P1", Descripcion = "Cable", Habilitado = true },
                new Piezas { Id = 2, IdSolicitud = 3, IdEmpresa = 2, Cantidad = 1, NoSerial = "P2", Descripcion = "Fuser", Habilitado = true });
            db.Seguimientos.AddRange(
                new Seguimiento { Id = 1, IdSolicitud = 1, IdEmpresa = 1, IdUsuario = 4, Texto = "Checked", Fecha = today, Hora = TimeSpan.FromHours(9), Habilitado = true },
                new Seguimiento { Id = 2, IdSolicitud = 3, IdEmpresa = 2, IdUsuario = 6, Texto = "Checked B", Fecha = today, Hora = TimeSpan.FromHours(9), Habilitado = true });
            db.SaveChanges();
        }

        private static Usuario User(int id, string account, string role, int company, string password) => new Usuario
        {
            Id = id, Nombre = account, CuentaUsuario = account, Acceso = role, IdEmpresa = company,
            Contrasena = password, Correo = account + "@test.local", NumDocumento = "000", Habilitado = true
        };

        public static Solicitud Ticket(int id, string number, int company, int user, int customer, DateTime created) => new Solicitud
        {
            Id = id, NoSecuencia = number, IdEmpresa = company, IdUsuario = user, IdCliente = customer,
            FechaCreacion = created, TipoSolicitud = "Servicio Taller", TipoServicio = "Garantía",
            Estado = "Abierto", Descripcion = "Ticket " + id, Habilitado = true
        };

        public T Query<T>(Func<HelpDeskDBContext, T> read)
        {
            using var scope = Services.CreateScope();
            return read(scope.ServiceProvider.GetRequiredService<HelpDeskDBContext>());
        }

        public async Task<string> LoginAsync(string account, string password = null)
        {
            var response = await CreateClient().PostAsync("/api/Login", Json(new { cuentaUsuario = account, contrasena = password ?? TestUsers.Password }));
            var body = await response.Content.ReadAsStringAsync();
            return string.IsNullOrEmpty(body) ? null : (string)JObject.Parse(body)["token"];
        }

        public async Task<HttpClient> ClientFor(string account)
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await LoginAsync(account));
            return client;
        }

        public static StringContent Json(object value) => new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");

        public class FakeMailService : IMailService
        {
            public List<(string To, string Subject, string Body)> Sent { get; } = new List<(string To, string Subject, string Body)>();

            public bool Send(string host, int port, SmtpDeliveryMethod method, bool useDefaultCredentials, bool enableSsl, string emailFrom, string password, string emailTo, string subject, string message, bool isBodyHtml)
            {
                lock (Sent) { Sent.Add((emailTo, subject, message)); }
                return true;
            }

            public bool TestConnection() => true;
        }
    }

    public static class TestUsers
    {
        public const string Password = "Passw0rd!";
    }
}
