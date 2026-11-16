using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace HelpDesk.Infrastructure.Validators
{
    // Lets the validators of FluentValidation (the Validators folder) take part in the validation that MVC does after it reads a body,
    // so what they find ends up in ModelState: under the name of the property that failed ("Nombre", or "[1].Marca" for the second item
    // of a list), with the message. That is where ValidationFilter, and the controllers marked [ApiController], read it from, and they
    // answer 400 when anything is there.
    public class FluentValidationModelValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            var type = context.ModelMetadata.ModelType;
            // Text, numbers and the like have no validator of their own, and a list is validated through its items.
            if (!type.IsClass || type == typeof(string) || type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return;
            }
            context.Results.Add(new ValidatorItem { Validator = new Validator(type), IsReusable = true });
        }

        private class Validator : IModelValidator
        {
            private readonly Type _modelType;

            public Validator(Type modelType)
            {
                _modelType = modelType;
            }

            public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
            {
                if (context.Model == null)
                {
                    return Array.Empty<ModelValidationResult>();
                }
                var validatorType = typeof(IValidator<>).MakeGenericType(_modelType);
                var validator = context.ActionContext.HttpContext.RequestServices.GetService(validatorType) as IValidator;
                if (validator == null)
                {
                    return Array.Empty<ModelValidationResult>();
                }
                var result = validator.Validate(new ValidationContext<object>(context.Model));
                var failures = new List<ModelValidationResult>();
                foreach (var failure in result.Errors)
                {
                    failures.Add(new ModelValidationResult(failure.PropertyName, failure.ErrorMessage));
                }
                return failures;
            }
        }
    }
}
