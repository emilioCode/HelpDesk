using HelpDesk.Core.CustomEntities;
using HelpDesk.Core.DTOs;
using HelpDesk.Tests.Integration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelpDesk.Tests.Validation
{
    // What every endpoint that takes a body answers to bodies that are wrong, written down as files: the status, the type of the answer
    // and its text. The validation library decides which answers come back (and in which form: the list of messages by property, or
    // the standard problem details of the controllers marked [ApiController]), so these files tell that an upgrade did not change them.
    // See GoldenFiles for how to record them again.
    [TestFixture]
    public class ValidationHttpGoldenTests
    {
        private ApiFactory _api;
        private HttpClient _admin;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _api = new ApiFactory(enforce: true);
            _api.Seed();
            _admin = await _api.ClientFor("admin");
        }

        [OneTimeTearDown]
        public void TearDown() => _api.Dispose();

        private static readonly MethodInfo VariantsMethod = typeof(GoldenFiles).GetMethod(nameof(GoldenFiles.Variants));

        // name, path, type of the body
        private static IEnumerable<(string Name, string Path, Type Body)> Endpoints()
        {
            yield return ("Costumer_Post", "/api/Costumer", typeof(ClienteDto));
            yield return ("Costumer_Put", "/api/Costumer/Put/1", typeof(ClienteDto));
            yield return ("Device_PostOne", "/api/Device/PostOne", typeof(EquipoDto));
            yield return ("Device_PostArray", "/api/Device/PostArray", typeof(List<EquipoDto>));
            yield return ("Device_Put", "/api/Device/Put", typeof(EquipoDto));
            yield return ("Business_Post", "/api/Business/1", typeof(EmpresaDto));
            yield return ("Business_Put", "/api/Business/Put/1", typeof(EmpresaDto));
            yield return ("Part_Post", "/api/Part", typeof(PiezasDto));
            yield return ("Part_Put", "/api/Part/Put", typeof(PiezasDto));
            yield return ("Ticket_Post", "/api/Ticket", typeof(SolicitudDto));
            yield return ("Ticket_Edit", "/api/Ticket/EDIT/ESTADO", typeof(SolicitudDto));
            yield return ("Ticket_Json", "/api/Ticket/GetJsonTicket", typeof(SolicitudLiteDto));
            yield return ("Trace_Post", "/api/Trace", typeof(SeguimientoDto));
            yield return ("Trace_Put", "/api/Trace/Put", typeof(SeguimientoDto));
            yield return ("User_Post", "/api/User", typeof(UsuarioDto));
            yield return ("User_Put", "/api/User/Put/1", typeof(UsuarioDto));
        }

        // The bodies tried: the situations of every kind of object, and the kinds of mistake a client can make with the text itself.
        private static List<(string Name, string Body)> Bodies(Type bodyType)
        {
            var bodies = new List<(string, string)>();
            if (bodyType.IsGenericType)
            {
                var element = bodyType.GetGenericArguments()[0];
                var variants = (System.Collections.IEnumerable)VariantsMethod.MakeGenericMethod(element).Invoke(null, null);
                var all = new List<object>();
                foreach (dynamic v in variants) { all.Add(v.Item2); }
                bodies.Add(("empty_list", "[]"));
                bodies.Add(("one_empty_item", "[{}]"));
                bodies.Add(("two_items", JsonConvert.SerializeObject(new[] { all[2], all[0] })));
                bodies.Add(("not_a_list", "{}"));
            }
            else
            {
                var variants = (System.Collections.IEnumerable)VariantsMethod.MakeGenericMethod(bodyType).Invoke(null, null);
                foreach (dynamic v in variants) { bodies.Add(((string)v.Item1, JsonConvert.SerializeObject(v.Item2))); }
                bodies.Add(("text_instead_of_a_number", "{\"id\":\"x\",\"idEmpresa\":\"x\"}"));
                bodies.Add(("a_list_instead_of_an_object", "[]"));
            }
            bodies.Add(("no_body", ""));
            bodies.Add(("null", "null"));
            bodies.Add(("a_text", "\"just text\""));
            bodies.Add(("broken_json", "{\"id\":"));
            return bodies;
        }

        // Everything that changes from one run to the next is replaced by a fixed text; a server error says no more than its status.
        private static string Normalize(string text) => Regex.Replace(Regex.Replace(Regex.Replace(text ?? "",
            "\"traceId\":\"[^\"]*\"", "\"traceId\":\"<trace>\""),
            "\\d{4}-\\d{2}-\\d{2}T[0-9:.]+Z?", "<date>"),
            "\"(token|expiresAt)\":\"[^\"]*\"", "\"$1\":\"<x>\"");

        [Test]
        public async Task EveryEndpointAnswersWhatWasRecorded()
        {
            var answers = new Dictionary<string, string>();
            foreach (var (name, path, bodyType) in Endpoints())
            {
                var results = new List<object>();
                foreach (var (bodyName, body) in Bodies(bodyType))
                {
                    var response = await _admin.PostAsync(path, new StringContent(body, Encoding.UTF8, "application/json"));
                    var status = (int)response.StatusCode;
                    var type = response.Content.Headers.ContentType?.MediaType;
                    var text = await response.Content.ReadAsStringAsync();
                    results.Add(new { body = bodyName, status, type, answer = status >= 500 ? "<server error>" : Normalize(text) });
                }
                answers[name] = JsonConvert.SerializeObject(results, Formatting.Indented) + "\n";
            }
            var problems = GoldenFiles.Compare("Http", answers);
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public async Task TheSignInForm_AnswersWhatWasRecorded()
        {
            // The sign-in is open to anyone: only the bodies that cannot match an account (they must not count as attempts of a real one).
            var client = _api.CreateClient();
            var results = new List<object>();
            foreach (var (bodyName, body) in new[] { ("empty_object", "{}"), ("no_body", ""), ("null", "null"), ("long_texts", JsonConvert.SerializeObject(new UserLogin { CuentaUsuario = new string('a', 300), Contrasena = new string('b', 300) })) })
            {
                var response = await client.PostAsync("/api/Login", new StringContent(body, Encoding.UTF8, "application/json"));
                var status = (int)response.StatusCode;
                results.Add(new { body = bodyName, status, type = response.Content.Headers.ContentType?.MediaType, answer = status >= 500 ? "<server error>" : Normalize(await response.Content.ReadAsStringAsync()) });
            }
            var problems = GoldenFiles.Compare("Http", new Dictionary<string, string> { ["Login_Post"] = JsonConvert.SerializeObject(results, Formatting.Indented) + "\n" });
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }
    }
}
