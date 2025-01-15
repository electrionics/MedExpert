using FluentValidation;

using MedExpert.Web.ViewModels.Account;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Validators
{
    public class AccountLoginFormModelValidator : AbstractValidator<LoginFormModel>
    {
        public AccountLoginFormModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите электронную почту.")
                .EmailAddress().WithMessage("Некорректый формат электронной почты.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль должен быть непустым.");
        }
    }
}