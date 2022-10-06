using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class PieceValidator : AbstractValidator<PiezasDto>
    {
        public PieceValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.Cantidad)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.NoSerial)
                .NotNull()
                .MaximumLength(50);
        }
    }
}
