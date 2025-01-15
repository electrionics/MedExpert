using FluentValidation;

using MedExpert.Excel.Model.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Common
{
    public class IntervalModelValidator:AbstractValidator<IntervalModel>
    {
        public IntervalModelValidator(string columnKey)
        {
            var rule1 = RuleFor(x => x.ValueMax)
                .GreaterThan(x => x.ValueMin)
                .WithMessage("Большее значение интервала должно превышать меньшее.");
            var rule2 = RuleFor(x => x.ValueMin)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Границы интервала доллжны быть неотрицательны.");
            
            if (columnKey != null)
            {
                rule1.WithName(columnKey);
                rule2.WithName(columnKey);
            }
        }
    }
}