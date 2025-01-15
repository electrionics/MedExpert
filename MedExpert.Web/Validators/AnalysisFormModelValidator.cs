using System.Linq;

using FluentValidation;

using MedExpert.Web.ViewModels.Analysis;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Validators
{
    public class AnalysisFormModelValidator:AbstractValidator<AnalysisFormModel>
    {
        public AnalysisFormModelValidator()
        {
            RuleFor(x => x.Profile)
                .SetValidator(new ProfileModelValidator());
            RuleForEach(x => x.Indicators)
                .SetValidator(new IndicatorValueModelValidator());
            RuleFor(x => x.Indicators)
                .Must(x => x.Any(y => y.Value != null))
                .WithMessage("Минимум одно значение показателя должно быть не пустым.");
            RuleFor(x => x.SpecialistIds)
                .NotEmpty().WithMessage("Список выбранных специалистов должен быть не пустым.");
        }
        
        private class ProfileModelValidator:AbstractValidator<ProfileModel>
        {
            public ProfileModelValidator()
            {
                RuleFor(x => x.Sex)
                    .IsInEnum().WithMessage("Некорректное значение пола.");
                RuleFor(x => x.Age)
                    .InclusiveBetween(0, 100).WithMessage("Возраст должен принимать значение от 0 до 100.");
            }
        }
        
        private class IndicatorValueModelValidator:AbstractValidator<IndicatorValueModel>
        {
            public IndicatorValueModelValidator()
            {
                RuleFor(x => x.ReferenceIntervalMin)
                    .NotEmpty().WithMessage("Введите нижнюю границу референсного интервала.")
                    .When(x => x.Value != null)
                    .GreaterThanOrEqualTo(0).WithMessage("Нижняя граница референсного интервала должна быть неотрицательной.")
                    .LessThan(x => x.ReferenceIntervalMax).WithMessage("Нижняя граница референсного интервала должна быть меньше верхней.");
                RuleFor(x => x.ReferenceIntervalMax)
                    .NotEmpty().WithMessage("Введите верхнюю границу референсного интервала.")
                    .When(x => x.Value != null)
                    .GreaterThan(0).WithMessage("Верхняя граница референсного интервала должна быть положительной.");
                RuleFor(x => x.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Значение показателя должно быть неотрицательным.");
            }
        }
    }
}