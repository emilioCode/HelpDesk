using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class TicketValidator : AbstractValidator<SolicitudDto>
    {
       public TicketValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.IdCliente)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.TipoSolicitud)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.TipoServicio)
                .NotEmpty()
                .NotNull();
        }
    }
}
