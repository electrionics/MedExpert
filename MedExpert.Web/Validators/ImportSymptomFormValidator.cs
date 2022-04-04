using FluentValidation;
using MedExpert.Services.Interfaces;
using MedExpert.Web.ViewModels;
using MedExpert.Web.ViewModels.Import;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Validators
{
    public class ImportSymptomFormValidator:AbstractValidator<ImportSymptomForm>
    {
        public ImportSymptomFormValidator(ISymptomCategoryService symptomCategoryService)
        {
            RuleFor(x => x.SpecialistId)
                .NotEmpty().WithMessage("Dыберите специалиста.")
                .When(x => string.IsNullOrEmpty(x.NewSpecialistName));
            RuleFor(x => x.SymptomCategoryId)
                .NotEmpty().WithMessage("Выберите категорию для строк.")
                .MustAsync(async (m, p, c) => await symptomCategoryService.CategoryExists(p))
                .WithMessage("Категория не существует.");
            RuleFor(x => x.NewSpecialistSex)
                .IsInEnum().WithMessage("Пол некорректен.")
                .When(x => x.NewSpecialistSex != null);
            RuleFor(x => x.NewSpecialistName)
                .NotEmpty().WithMessage("Dведите название нового специалиста.")
                .MaximumLength(100).WithMessage("Длина названия не должна превышать 100 символов.")
                .Matches("^[a-zA-Zа-яА-Я0-9-,() ]*$").WithMessage("Название содержит недопустимые символы.")
                .When(x => x.SpecialistId == null);
        }
    }
}