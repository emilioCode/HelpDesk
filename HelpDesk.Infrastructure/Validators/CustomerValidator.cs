using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class CustomerValidator : AbstractValidator<ClienteDto>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Nombre)
                .NotNull()
                .MaximumLength(100);

            RuleFor(x => x.IdEmpresa)
                .NotNull()
                .GreaterThan(0);
        }
    }
}
