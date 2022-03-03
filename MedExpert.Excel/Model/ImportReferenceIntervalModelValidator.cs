using FluentValidation;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Excel.Model
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