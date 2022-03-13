using MedExpert.Domain.Enums;

namespace MedExpert.Excel.Model.Symptoms
{
    public class SymptomNameModel
    {
        public string Value { get; set; }
        
        public string SexStr { get; set; }
        
        public Sex? Sex { get; set; }
    }
}