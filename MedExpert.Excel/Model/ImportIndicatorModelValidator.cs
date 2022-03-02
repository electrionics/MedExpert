using FluentValidation;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
{
    public class ImportIndicatorModelValidator:AbstractValidator<ImportIndicatorModel>
    {
        public ImportIndicatorModelValidator()
        {
            RuleFor(x => x.ShortName)
                .NotEmpty().WithMessage("Сокращенное название должно быть непустым.")
                .MaximumLength(50).WithMessage("Длина не должна превышать 50 символов.");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название должно быть непустым.")
                .MaximumLength(100).WithMessage("Длина не должна превышать 100 символов.");
            RuleFor(x => x.FormulaType)
                .IsInEnum().WithMessage("Некорректная формула вычисления показателя.");
        }
    }
}