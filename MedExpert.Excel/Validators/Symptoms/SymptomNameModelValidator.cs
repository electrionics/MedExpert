using FluentValidation;

using MedExpert.Excel.Model.Symptoms;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Symptoms
{
    public class SymptomNameModelValidator:AbstractValidator<SymptomNameModel>
    {
        public SymptomNameModelValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Название симптома(болезни) не должно быть пустым.")
                .MaximumLength(300).WithMessage("Название болезни не должно превышать 300 символов.")
                .WithName("SymptomName");
            RuleFor(x => x.SexStr)
                .Must(x => x is "М" or "Ж" || string.IsNullOrEmpty(x))
                .WithMessage("Значение пола должно быть равно 'М' или 'Ж'.")
                .WithName("SymptomName");
        }
    }
}