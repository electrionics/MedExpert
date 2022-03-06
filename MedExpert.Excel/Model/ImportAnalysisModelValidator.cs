using FluentValidation;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
{
    public class ImportAnalysisModelValidator:AbstractValidator<ImportAnalysisModel>
    {
        public ImportAnalysisModelValidator()
        {
            RuleFor(x => x.Sex)
                .IsInEnum().WithMessage("Пол указан некорректно.");
            RuleFor(x => x.Age)
                .NotEmpty()
                .WithMessage("Укажите возраст.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("Возраст должен быть неотрицательным значением.");
        }
    }
}