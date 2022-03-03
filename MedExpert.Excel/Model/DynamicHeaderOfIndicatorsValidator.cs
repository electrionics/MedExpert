using System.Collections.Generic;
using FluentValidation;
using MedExpert.Services.Interfaces;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
{
    public class DynamicHeaderOfIndicatorsValidator:AbstractValidator<List<string>>
    {
        public DynamicHeaderOfIndicatorsValidator(IIndicatorService indicatorService)
        {
            RuleFor(x => x)
                .MustAsync(async (list, _) => await indicatorService.AllShortNamesExists(list))
                .WithMessage("Не все показатели из заголовка файла существуют.");
        }
    }
}