using FluentValidation;

using MedExpert.Excel.Model.ReferenceIntervals;
using MedExpert.Excel.Validators.Common;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Validators.ReferenceIntervals
{
    public class ImportReferenceIntervalModelValidator:AbstractValidator<ImportReferenceIntervalModel>
    {
        public ImportReferenceIntervalModelValidator()
        {
            RuleFor(x => x.Sex)
                .IsInEnum().WithMessage("Пол указан некорректно.");
            RuleFor(x => x.AgeInterval)
                .SetValidator(new IntervalModelValidator(null));
            RuleForEach(x => x.Values)
                .SetValidator(new IntervalKeyValueModelValidator());
        }
    }
}