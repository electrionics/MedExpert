using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MedExpert.Services.Interfaces;

namespace MedExpert.Excel.Model
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