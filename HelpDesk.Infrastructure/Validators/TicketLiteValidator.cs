using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class TicketLiteValidator : AbstractValidator<SolicitudLiteDto>
    {
        public TicketLiteValidator()
        {
            RuleFor(x => x.IdEmpresa)
                .NotEmpty()
                .NotNull()
                .GreaterThan(0);
        }
    }
}
