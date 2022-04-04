using MedExpert.Domain.Enums;

namespace MedExpert.Web.ViewModels.Import
{
    public class ImportSymptomForm
    {
        public int? SpecialistId { get; set; }
        
        public int SymptomCategoryId { get; set; }
        
        public string NewSpecialistName { get; set; }
        
        public Sex? NewSpecialistSex { get; set; }
    }
}