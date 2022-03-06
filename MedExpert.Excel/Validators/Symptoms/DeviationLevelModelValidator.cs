using FluentValidation;
using MedExpert.Excel.Model.Symptoms;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Symptoms
{
    public class DeviationLevelModelValidator:AbstractValidator<DeviationLevelModel>
    {
        public DeviationLevelModelValidator(string columnName)
        {
            var rule1 = RuleFor(x => x.Alias)
                .Must((m, p) => m.AllowedAliases.Contains(p))
                .WithMessage(x => $"Недопустимое значение уровня отклонения: '{x.Alias}'.");

            if (columnName != null)
            {
                rule1.WithName(columnName);
            }
        }
    }
}