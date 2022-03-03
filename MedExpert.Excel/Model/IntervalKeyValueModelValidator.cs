using System.Collections.Generic;
using FluentValidation;

namespace MedExpert.Excel.Model
{
    public class IntervalKeyValueModelValidator:AbstractValidator<KeyValuePair<string, IntervalModel>>
    {
        public IntervalKeyValueModelValidator()
        {
            RuleFor(x => x.Value)
                .SetValidator(x => new IntervalModelValidator(x.Key));
        }
    }
}