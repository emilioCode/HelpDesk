using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Integration
{
    // Approving a ticket notifies the customer. The company data in that e-mail is optional.
    public class ApprovalEmailTests
    {
        private ApiFactory _api;

        [OneTimeSetUp]
        public void SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
            // Own rows, so other tests that count tickets and customers are not affected.
            _api.Query(db =>
            {
                var today = System.DateTime.Now.Date;
                db.Clientes.Add(new HelpDesk.Core.Entities.Cliente { Id = 4, Nombre = "Customer without mail", Correo = null, IdEmpresa = 1, Habilitado = true });
                db.Solicitudes.AddRange(
                    ApiFactory.Ticket(20, "20", 2, 6, 2, today),   // company without logo, address, phone or RNC
                    ApiFactory.Ticket(21, "21", 1, 4, 4, today),   // customer without e-mail
                    ApiFactory.Ticket(22, "22", 1, 4, 1, today));  // company with logo and address
                db.SaveChanges();
                return 0;
            });
        }

        [OneTimeTearDown]
        public void TearDown() => _api.Dispose();

        private async Task<HttpStatusCode> Approve(string user, int id, int company, int customer)
        {
            var client = await _api.ClientFor(user);
            var body = new { id, noSecuencia = id.ToString(), idEmpresa = company, idUsuario = 0, idCliente = customer, tipoSolicitud = "Servicio Taller", tipoServicio = "Garantía", estado = "Completado" };
            var response = await client.PostAsync("/api/Ticket/EDIT/APROBAR", ApiFactory.Json(body));
            return response.StatusCode;
        }

        [Test]
        public async Task ACompanyWithoutLogoAddressPhoneOrRnc_CanStillApprove_AndTheMailShowsItsName()
        {
            var status = await Approve("adminb", 20, 2, 2);

            Assert.AreEqual(HttpStatusCode.OK, status);
            var mail = _api.Mail.Sent.Single(m => m.To == "cb@test.local");
            StringAssert.Contains("<h2>Company B</h2>", mail.Body, "the name replaces the logo");
            StringAssert.DoesNotContain("<img", mail.Body);
            StringAssert.DoesNotContain("Tel:", mail.Body);
            StringAssert.DoesNotContain("RNC:", mail.Body);
        }

        [Test]
        public async Task ACustomerWithoutAnEmail_DoesNotStopTheApproval_AndNothingIsSent()
        {
            var before = _api.Mail.Sent.Count;

            var status = await Approve("mod", 21, 1, 4);

            Assert.AreEqual(HttpStatusCode.OK, status);
            Assert.AreEqual(before, _api.Mail.Sent.Count);
            Assert.IsNotNull(_api.Query(db => db.Solicitudes.Single(t => t.Id == 21).AprobadoPor));
        }

        [Test]
        public async Task ACompanyWithLogoAndAddress_KeepsThemInTheMail()
        {
            var status = await Approve("mod", 22, 1, 1);

            Assert.AreEqual(HttpStatusCode.OK, status);
            var mail = _api.Mail.Sent.Single(m => m.To == "ca@test.local");
            StringAssert.Contains("<img src='data:image/jpeg;base64,", mail.Body);
            StringAssert.Contains("Main street 1", mail.Body);
            StringAssert.Contains("Tel: 555-0100", mail.Body);
        }
    }
}
