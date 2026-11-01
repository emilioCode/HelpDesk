using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class UserValidator : AbstractValidator<UsuarioDto>
    {
        public UserValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .NotNull()
                .MaximumLength(255);

            RuleFor(x => x.NumDocumento)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(x => x.CuentaUsuario)
                .NotEmpty()
                .NotNull()
                .MaximumLength(20);

            // Required to create a user. When editing, an empty password keeps the current one.
            RuleFor(x => x.Contrasena)
                .NotNull()
                .NotEmpty()
                .When(x => x.Id == 0);
            RuleFor(x => x.Contrasena)
                .MinimumLength(6)
                .When(x => !string.IsNullOrEmpty(x.Contrasena));

            RuleFor(x => x.Acceso)
                .NotEmpty()
                .NotNull()
                .MaximumLength(20);

            RuleFor(x => x.Correo)
                .MaximumLength(50)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

            RuleFor(x => x.IdEmpresa)
                .NotNull()
                .GreaterThan(0);
        }
    }
}
