using System.Collections.Generic;

using FluentValidation;

using MedExpert.Excel.Model.Common;
using MedExpert.Excel.Validators.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.ReferenceIntervals
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