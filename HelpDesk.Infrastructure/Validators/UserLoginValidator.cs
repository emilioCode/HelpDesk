using FluentValidation;
using HelpDesk.Core.CustomEntitties;

namespace HelpDesk.Infrastructure.Validators
{
    public class UserLoginValidator : AbstractValidator<UserLogin>
    {
        public UserLoginValidator()
        {
            RuleFor(x => x.CuentaUsuario)
                .NotEmpty()
                .NotNull();

            RuleFor(x => x.Contrasena)
                .NotEmpty()
                .NotNull();
        }
    }
}
