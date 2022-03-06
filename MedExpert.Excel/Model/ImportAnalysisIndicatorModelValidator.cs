using System;
using System.Linq;
using FluentValidation;
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace MedExpert.Excel.Model
{
    public class ImportAnalysisIndicatorModelValidator:AbstractValidator<ImportAnalysisIndicatorModel>
    {
        public ImportAnalysisIndicatorModelValidator()
        {
            RuleFor(x => x.Indicator)
                .Must((m, p) => m.AllowedIndicatorShortNames.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage(x => $"Показатель с кратким названием '{x.Indicator}' не существует.");
            // RuleFor(x => x.Value)
            //     .NotEmpty()
            //     .WithMessage(x => $"Значение показателя '{x.Indicator}' не должно быть пустым.")
            //     .When(x => !x.Calculated);
            RuleFor(x => x.ReferenceInterval)
                .SetValidator(new IntervalModelValidator(null));
        }
    }
}