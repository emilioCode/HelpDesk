using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class BusinessValidator : AbstractValidator<EmpresaDto>
    {
        public BusinessValidator()
        {
            RuleFor(x => x.RazonSocial)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.SectorComercial)
                .MaximumLength(50);

            RuleFor(x => x.Rnc)
                .MaximumLength(50);

            RuleFor(x => x.Telefono)
                .MaximumLength(50);

            RuleFor(x => x.Correo)
                .MaximumLength(50)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

            RuleFor(x => x.Contrasena)
                .MaximumLength(30);

            RuleFor(x => x.Url)
                .MaximumLength(80);

            RuleFor(x => x.Host)
                .MaximumLength(30);

            RuleFor(x => x.Direccion)
                .MaximumLength(150);
            
            RuleFor(x => x.NoAutorizacion)
                .MaximumLength(12);

            RuleFor(x => x.Secuenciaticket)
                .MaximumLength(12)
                .NotEmpty()
                .NotNull();
        }
    }
}
