using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class TraceValidator : AbstractValidator<SeguimientoDto>
    {
        public TraceValidator()
        {
            RuleFor(x => x.Texto)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.IdEmpresa)
                .NotEmpty()
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.IdSolicitud)
                .NotEmpty()
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.IdUsuario)
                .NotEmpty()
                .NotNull()
                .GreaterThan(0);
        }
    }
}
