using System.Collections.Generic;
using FluentValidation;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
{
    public class IntervalKeyValueModelValidator:AbstractValidator<KeyValuePair<string, IntervalModel>>
    {
        public IntervalKeyValueModelValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Значение не должно быть пустым").WithName(x => x.Key)
                .SetValidator(x => new IntervalModelValidator(x.Key));
        }
    }
}