using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HelpDesk.Tests.Validation
{
    // Answers written down as files and compared on every run.
    // To record them again (only when the behaviour is meant to change): set GOLDEN_RECORD=1 and GOLDEN_ROOT to the folder
    // HelpDesk.Tests of the source code. The files live in Validation/Golden/<group>/<name>.json.
    public static class GoldenFiles
    {
        private static string Folder(string group)
        {
            var root = Environment.GetEnvironmentVariable("GOLDEN_ROOT");
            return root != null
                ? Path.Combine(root, "Validation", "Golden", group)
                : Path.Combine(AppContext.BaseDirectory, "Validation", "Golden", group);
        }

        public static bool Recording => Environment.GetEnvironmentVariable("GOLDEN_RECORD") == "1";

        // Writes the answers (in record mode) or says what differs from the recorded ones.
        public static List<string> Compare(string group, IDictionary<string, string> answers)
        {
            var problems = new List<string>();
            var folder = Folder(group);
            if (Recording)
            {
                Directory.CreateDirectory(folder);
                foreach (var answer in answers) { File.WriteAllText(Path.Combine(folder, answer.Key + ".json"), answer.Value); }
                return problems;
            }
            foreach (var answer in answers)
            {
                var file = Path.Combine(folder, answer.Key + ".json");
                if (!File.Exists(file)) { problems.Add(answer.Key + ": no recorded file"); continue; }
                if (File.ReadAllText(file) != answer.Value) { problems.Add(answer.Key + ": the answer differs from the recorded one"); }
            }
            return problems;
        }

        // ---- the objects to validate

        // Every property gets a value that tells which property it is; the e-mail is a real one.
        public static T Full<T>(int seed = 10, string textOf = null, int? number = null) where T : new()
        {
            var item = new T();
            foreach (var p in typeof(T).GetProperties().OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                if (!p.CanWrite) { continue; }
                seed++;
                var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                object value =
                    type == typeof(string) ? (textOf != null ? textOf : (p.Name.Contains("Correo") ? "name@test.local" : p.Name + "-text")) :
                    type == typeof(int) ? (number ?? seed) :
                    type == typeof(bool) ? true :
                    type == typeof(DateTime) ? new DateTime(2026, 3, 4) :
                    type == typeof(TimeSpan) ? new TimeSpan(1, 2, 3) :
                    type == typeof(byte[]) ? new byte[] { 1, 2, 3 } :
                    null;
                p.SetValue(item, value);
            }
            return item;
        }

        public static T With<T>(T item, string property, object value)
        {
            typeof(T).GetProperty(property).SetValue(item, value);
            return item;
        }

        // The situations tried for each kind of object, by name.
        public static List<(string Name, object Item)> Variants<T>() where T : new()
        {
            var mail = typeof(T).GetProperty("Correo") != null;
            var list = new List<(string, object)>
            {
                ("nothing_filled", new T()),
                ("everything_filled", Full<T>()),
                ("long_texts", Full<T>(textOf: new string('a', 300))),
                ("blank_texts", Full<T>(textOf: "   ")),
                ("empty_texts", Full<T>(textOf: "")),
                ("zero_numbers", Full<T>(number: 0)),
                ("negative_numbers", Full<T>(number: -1)),
            };
            if (mail) { list.Add(("bad_email", With(Full<T>(), "Correo", "not-an-email"))); }
            return list;
        }

        // The types that have a validator, found the way the application finds them.
        public static IEnumerable<Type> ValidatorTypes()
        {
            return typeof(HelpDesk.Infrastructure.Extensions.ServiceCollectionExtension).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition().Name.StartsWith("AbstractValidator"))
                .OrderBy(t => t.Name, StringComparer.Ordinal);
        }
    }
}
