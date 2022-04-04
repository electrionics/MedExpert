using FluentValidation;
using MedExpert.Web.ViewModels;
using MedExpert.Web.ViewModels.Import;

namespace MedExpert.Web.Validators
{
    public class ReferenceIntervalModelValidator:AbstractValidator<ReferenceIntervalModel>
    {
        public ReferenceIntervalModelValidator()
        {
        }
    }
}