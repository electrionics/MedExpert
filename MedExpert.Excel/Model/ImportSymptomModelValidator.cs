using System.Linq;
using FluentValidation;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
{
    public class ImportSymptomModelValidator: AbstractValidator<ImportSymptomModel>
    {
        public ImportSymptomModelValidator()
        {
            RuleFor(x => x.SymptomName)
                .NotEmpty().WithMessage("Название симптома(болезни) не должно быть пустым.")
                .MaximumLength(300).WithMessage("Название болезни не должно превышать 300 символов.");

            RuleFor(x => x.DeviationLevels)
                .NotEmpty().WithMessage(x =>
                    $"Болезнь/симптом '{x.SymptomName}' должен содержать как минимум один показатель с указанным уровнем отклонения.")
                .WithName("SymptomName");
            
            RuleForEach(x => x.DeviationLevels)
                .SetValidator(new DeviationLevelKeyValueModelValidator());
        }
    }
}