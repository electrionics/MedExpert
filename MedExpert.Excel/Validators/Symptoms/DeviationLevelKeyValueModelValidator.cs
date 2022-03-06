using System.Collections.Generic;
using FluentValidation;
using MedExpert.Excel.Model;
using MedExpert.Excel.Model.Symptoms;

namespace MedExpert.Excel.Validators.Symptoms
{
    public class DeviationLevelKeyValueModelValidator:AbstractValidator<KeyValuePair<string, DeviationLevelModel>>
    {
        public DeviationLevelKeyValueModelValidator()
        {
            RuleFor(x => x.Value)
                .SetValidator(x => new DeviationLevelModelValidator(x.Key));
        }
    }
}