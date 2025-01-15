using FluentValidation;

using MedExpert.Excel.Model.Symptoms;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Symptoms
{
    public class ImportSymptomModelValidator: AbstractValidator<ImportSymptomModel>
    {
        public ImportSymptomModelValidator()
        {
            RuleFor(x => x.SymptomName)
                .NotNull().WithMessage("Название симптома(болезни) не должно быть пустым.")
                .SetValidator(new SymptomNameModelValidator());

            RuleFor(x => x.DeviationLevels)
                .NotEmpty().WithMessage(x =>
                    $"Болезнь/симптом '{x.SymptomName.Value}' должен содержать как минимум один показатель с указанным уровнем отклонения.")
                .WithName("SymptomName");
            
            RuleForEach(x => x.DeviationLevels)
                .SetValidator(new DeviationLevelKeyValueModelValidator());
        }
    }
}