using FluentValidation;
using MedExpert.Web.ViewModels.Account;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Validators
{
    public class AccountRegisterFormModelValidator:AbstractValidator<RegisterFormModel>
    {
        public AccountRegisterFormModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Электронная почта должна быть непустой.")
                .EmailAddress().WithMessage("Некорректый формат электронной почты.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль должен быть непустым.")
                .Length(6, 100).WithMessage(" Пароль должен содержать от 6 до 100 символов.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Пароль и подтверждение пароля должны совпадать.");
        }
    }
}