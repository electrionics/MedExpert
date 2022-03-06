using System.Linq;
using FluentValidation;

namespace MedExpert.Excel.Model
{
    public class ImportAnalysisDeviationLevelModelValidator:AbstractValidator<ImportAnalysisDeviationLevelModel>
    {
        public ImportAnalysisDeviationLevelModelValidator()
        {
            RuleFor(x => x.Alias)
                .Must((m, p) => m.AllowedAliases.Contains(p))
                .WithMessage(p => $"Уровень отклонения {p.Alias} недопустим.");
            RuleFor(x => x.MinPercentFromCenter)
                .NotEmpty()
                .WithMessage("Процент вниз от центра не должен быть пустым.")
                .When(x => x.DeviationLevelId <= 0 && x.DeviationLevelId > x.AllowedDeviationLevelIds.Min());
            RuleFor(x => x.MinPercentFromCenter)
                .Empty()
                .WithMessage("Процент вниз от центра должен быть пустым.")
                .When(x => x.DeviationLevelId == x.AllowedDeviationLevelIds.Min() || x.DeviationLevelId > 0);
            RuleFor(x => x.MaxPercentFromCenter)
                .NotEmpty()
                .WithMessage("Процент вверх от центра не должен быть пустым.")
                .When(x => x.DeviationLevelId >= 0 && x.DeviationLevelId < x.AllowedDeviationLevelIds.Max());
            RuleFor(x => x.MaxPercentFromCenter)
                .Empty()
                .WithMessage("Процент вверх от центра должен быть пустым.")
                .When(x => x.DeviationLevelId == x.AllowedDeviationLevelIds.Max() || x.DeviationLevelId < 0);
        }
    }
}