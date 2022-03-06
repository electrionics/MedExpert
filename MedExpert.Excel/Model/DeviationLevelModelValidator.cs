using System.Collections.Generic;
using FluentValidation;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
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