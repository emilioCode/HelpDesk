using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HelpDesk.Tests.Validation
{
    // What every validator says (which property, which message, which code) about the same objects, written down as files.
    // They were recorded with the version of the validation library that existed before the upgrade, so a newer version must say
    // exactly the same. See GoldenFiles for how to record them again.
    [TestFixture]
    public class ValidatorGoldenTests
    {
        private static readonly MethodInfo VariantsMethod = typeof(GoldenFiles).GetMethod(nameof(GoldenFiles.Variants));

        [Test]
        public void EveryValidatorSaysWhatWasRecorded()
        {
            var answers = new Dictionary<string, string>();
            foreach (var validatorType in GoldenFiles.ValidatorTypes())
            {
                var modelType = validatorType.BaseType.GetGenericArguments()[0];
                var validator = Activator.CreateInstance(validatorType);
                // Validate(T) exists in every version of the library, the other overloads do not.
                var validate = validatorType.GetMethod("Validate", new[] { modelType });
                var variants = (System.Collections.IEnumerable)VariantsMethod.MakeGenericMethod(modelType).Invoke(null, null);
                var results = new List<object>();
                foreach (dynamic variant in variants)
                {
                    object item = variant.Item2;
                    string name = variant.Item1;
                    var result = (ValidationResult)validate.Invoke(validator, new[] { item });
                    results.Add(new
                    {
                        situation = name,
                        valid = result.IsValid,
                        failures = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage, e.ErrorCode }).ToList()
                    });
                }
                answers[validatorType.Name] = JsonConvert.SerializeObject(results, Formatting.Indented) + "\n";
            }
            Assert.That(answers, Is.Not.Empty, "no validator was found");
            var problems = GoldenFiles.Compare("Validators", answers);
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }
    }
}
