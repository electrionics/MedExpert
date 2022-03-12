using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using MedExpert.Services.Interfaces;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.Common
{
    public class DynamicHeaderOfIndicatorsValidator:AbstractValidator<List<string>>
    {
        public DynamicHeaderOfIndicatorsValidator(IIndicatorService indicatorService)
        {
            RuleFor(x => x)
                .Custom((list, context) =>
                {
                    var repeatedList = GetRepeatedItems(list);
                    if (repeatedList.Any())
                    {
                        context.AddFailure(
                            $"Заголовки столбцов с названиями {string.Join(", ", repeatedList)} повторяются.");
                    }
                })
                .CustomAsync(async (list, context, _) =>
                {
                    var notExistsList = await indicatorService.GetShortNamesNotExists(list);
                    if (notExistsList.Any())
                    {
                        context.AddFailure(
                            $"Не все показатели из заголовка файла существуют: {string.Join(", ", notExistsList)}.");
                    }
                });
        }

        private static List<string> GetRepeatedItems(IEnumerable<string> list)
        {
            return list
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();
        }
    }
}