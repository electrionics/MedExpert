using FluentValidation;
using MedExpert.Excel.Model.Analysis;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Analysis
{
    public class ImportAnalysisModelValidator:AbstractValidator<ImportAnalysisModel>
    {
        public ImportAnalysisModelValidator()
        {
            RuleFor(x => x.Sex)
                .IsInEnum().WithMessage("Пол указан некорректно.");
            RuleFor(x => x.Age)
                .NotEmpty().WithMessage("Укажите возраст.")
                .GreaterThanOrEqualTo(0).WithMessage("Возраст должен быть неотрицательным значением.");
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Дата анализа обязательна.");
        }
    }
}