using FluentValidation;
using HelpDesk.Core.DTOs;

namespace HelpDesk.Infrastructure.Validators
{
    public class DeviceValidator : AbstractValidator<EquipoDto>
    {
        public DeviceValidator()
        {
            RuleFor(x => x.IdSolicitud)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.IdEmpresa)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.FallaReportada)
                .MaximumLength(80)
                .NotNull();

            RuleFor(x => x.Marca)
                .MaximumLength(50)
                .NotNull();

            RuleFor(x => x.Modelo)
                .MaximumLength(50)
                .NotNull();

            RuleFor(x => x.NoSerial)
                .MaximumLength(50)
                .NotNull();
        }
    }
}
