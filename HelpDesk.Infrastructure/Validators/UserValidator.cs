using FluentValidation;
using HelpDesk.Core.Constants;
using HelpDesk.Core.DTOs;
using System;

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

            // The root user is the account of the system and has no document (the initial script creates it without one),
            // so everybody else needs it but root does not; otherwise root could not save its own profile.
            RuleFor(x => x.NumDocumento)
                .NotEmpty()
                .When(x => !string.Equals(x.Acceso, Roles.Root, StringComparison.OrdinalIgnoreCase));
            RuleFor(x => x.NumDocumento)
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
                .EmailAddress();

            RuleFor(x => x.IdEmpresa)
                .NotNull()
                .GreaterThan(0);
        }
    }
}
