using AutoMapper;
using HelpDesk.Core.DTOs;
using HelpDesk.Core.Entities;
using HelpDesk.Infrastructure.Mappings;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HelpDesk.Tests.Mappings
{
    // What every mapping between an entity and its DTO produces, written down as files (Mappings/Golden/*.json).
    // Each case maps a full object, an object with every optional value empty, nothing at all (null) and, for lists, an empty list and
    // a list of two, and compares the result with the file. The files were recorded from the mapper that existed before the change of
    // library, so a different mapper must give exactly the same answers.
    // To record them again (only when the mapping is meant to change): set GOLDEN_RECORD=1 and GOLDEN_DIR to the folder of the files.
    [TestFixture]
    public class MappingGoldenTests
    {
        private IMapper _mapper;

        [OneTimeSetUp]
        public void SetUp()
        {
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>()).CreateMapper();
        }

        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        // ---- objects to map

        // Every property gets a value that tells which property it is, so a value that lands in the wrong one shows.
        private static T Full<T>() where T : new()
        {
            var item = new T();
            var seed = 10;
            foreach (var p in typeof(T).GetProperties().OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                if (!p.CanWrite) { continue; }
                seed++;
                var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                object value =
                    type == typeof(string) ? p.Name + "-text" :
                    type == typeof(int) ? seed :
                    type == typeof(bool) ? true :
                    type == typeof(DateTime) ? new DateTime(2026, 3, 4, 5, 6, 7) :
                    type == typeof(TimeSpan) ? new TimeSpan(1, 2, 3) :
                    type == typeof(byte[]) ? new byte[] { 1, 2, (byte)seed } :
                    null;
                p.SetValue(item, value);
            }
            return item;
        }

        // Nothing optional is filled in: text, numbers with no value and files stay null.
        private static T Empty<T>() where T : new() => new T();

        private static T With<T>(T item, string property, object value) where T : new()
        {
            typeof(T).GetProperty(property).SetValue(item, value);
            return item;
        }

        // ---- the cases: the calls the controllers make

        private IEnumerable<(string Name, Func<object, object> Map, List<object> Inputs)> Cases()
        {
            yield return ("EquipoDto_to_Equipo", x => _mapper.Map<Equipo>((EquipoDto)x), new List<object> { Full<EquipoDto>(), Empty<EquipoDto>(), null });
            yield return ("Equipo_to_EquipoDto", x => _mapper.Map<EquipoDto>((Equipo)x), new List<object> { Full<Equipo>(), Empty<Equipo>(), null });
            yield return ("EquipoDtoList_to_EquipoList", x => _mapper.Map<List<Equipo>>((List<EquipoDto>)x), Lists<EquipoDto>());
            yield return ("EquipoEnumerable_to_EquipoDtoEnumerable", x => _mapper.Map<IEnumerable<EquipoDto>>((IEnumerable<Equipo>)x), Lists<Equipo>());

            yield return ("UsuarioDto_to_Usuario", x => _mapper.Map<Usuario>((UsuarioDto)x), new List<object> { Full<UsuarioDto>(), Empty<UsuarioDto>(), With(Full<UsuarioDto>(), "IdEmpresa", 0), With(Full<UsuarioDto>(), "Image", null), null });
            yield return ("Usuario_to_UsuarioDto", x => _mapper.Map<UsuarioDto>((Usuario)x), new List<object> { Full<Usuario>(), Empty<Usuario>(), With(Full<Usuario>(), "IdEmpresa", 0), With(Full<Usuario>(), "Image", null), null });
            yield return ("UsuarioEnumerable_to_UsuarioDtoEnumerable", x => _mapper.Map<IEnumerable<UsuarioDto>>((IEnumerable<Usuario>)x), Lists<Usuario>());

            yield return ("SeguimientoDto_to_Seguimiento", x => _mapper.Map<Seguimiento>((SeguimientoDto)x), new List<object> { Full<SeguimientoDto>(), Empty<SeguimientoDto>(), null });
            yield return ("Seguimiento_to_SeguimientoDto", x => _mapper.Map<SeguimientoDto>((Seguimiento)x), new List<object> { Full<Seguimiento>(), Empty<Seguimiento>(), null });

            yield return ("ClienteDto_to_Cliente", x => _mapper.Map<Cliente>((ClienteDto)x), new List<object> { Full<ClienteDto>(), Empty<ClienteDto>(), null });
            yield return ("ClienteEnumerable_to_ClienteDtoEnumerable", x => _mapper.Map<IEnumerable<ClienteDto>>((IEnumerable<Cliente>)x), Lists<Cliente>());

            yield return ("EmpresaDto_to_Empresa", x => _mapper.Map<Empresa>((EmpresaDto)x), new List<object> { Full<EmpresaDto>(), Empty<EmpresaDto>(), With(Full<EmpresaDto>(), "Image", null), null });
            yield return ("Empresa_to_EmpresaDto", x => _mapper.Map<EmpresaDto>((Empresa)x), new List<object> { Full<Empresa>(), Empty<Empresa>(), With(Full<Empresa>(), "Image", null), null });
            yield return ("EmpresaEnumerable_to_EmpresaDtoEnumerable", x => _mapper.Map<IEnumerable<EmpresaDto>>((IEnumerable<Empresa>)x), Lists<Empresa>());

            yield return ("SolicitudDto_to_Solicitud", x => _mapper.Map<Solicitud>((SolicitudDto)x), new List<object> { Full<SolicitudDto>(), Empty<SolicitudDto>(), null });
            yield return ("SolicitudLiteDto_to_Solicitud", x => _mapper.Map<Solicitud>((SolicitudLiteDto)x), new List<object> { Full<SolicitudLiteDto>(), Empty<SolicitudLiteDto>(), null });

            yield return ("PiezasDto_to_Piezas", x => _mapper.Map<Piezas>((PiezasDto)x), new List<object> { Full<PiezasDto>(), Empty<PiezasDto>(), null });
            yield return ("Piezas_to_PiezasDto", x => _mapper.Map<PiezasDto>((Piezas)x), new List<object> { Full<Piezas>(), Empty<Piezas>(), null });
            yield return ("PiezasEnumerable_to_PiezasDtoEnumerable", x => _mapper.Map<IEnumerable<PiezasDto>>((IEnumerable<Piezas>)x), Lists<Piezas>());
        }

        // Lists of the source type: two items, none, and no list at all. They are handed over as the type each call expects.
        private static List<object> Lists<T>() where T : new()
        {
            return new List<object> { new List<T> { Full<T>(), Empty<T>() }, new List<T>(), null };
        }

        private static string GoldenFolder => Environment.GetEnvironmentVariable("GOLDEN_DIR") ?? Path.Combine(AppContext.BaseDirectory, "Mappings", "Golden");

        // What a mapping returned for each input, as text. A call that throws is written down as the error it raised.
        private static string Describe(Func<object, object> map, List<object> inputs)
        {
            var results = new List<object>();
            foreach (var input in inputs)
            {
                try
                {
                    var output = map(input);
                    // A list is written as a list whatever its concrete type is.
                    results.Add(new { input, output = output is IEnumerable e && !(output is string) ? e.Cast<object>().ToList() : output });
                }
                catch (Exception ex)
                {
                    results.Add(new { input, error = ex.GetType().Name });
                }
            }
            return JsonConvert.SerializeObject(results, Json) + "\n";
        }

        [Test]
        public void EveryMappingGivesTheRecordedAnswer()
        {
            var record = Environment.GetEnvironmentVariable("GOLDEN_RECORD") == "1";
            var problems = new List<string>();
            foreach (var (name, map, inputs) in Cases())
            {
                var actual = Describe(map, inputs);
                var file = Path.Combine(GoldenFolder, name + ".json");
                if (record)
                {
                    Directory.CreateDirectory(GoldenFolder);
                    File.WriteAllText(file, actual);
                    continue;
                }
                if (!File.Exists(file)) { problems.Add(name + ": no recorded file"); continue; }
                var expected = File.ReadAllText(file);
                if (expected != actual) { problems.Add(name + ": the answer differs from the recorded one"); }
            }
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }
    }
}
