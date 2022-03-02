using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;

namespace MedExpert.Excel.Model
{
    public class ImportIndicatorModel
    {
        public string ShortName { get; set; }
        
        public string Name { get; set; }
        
        public bool InAnalysis { get; set; }
        
        public FormulaType? FormulaType { get; set; }

        public void UpdateEntity(Indicator indicator)
        {
            indicator.Name = Name;
            indicator.InAnalysis = InAnalysis;
            indicator.FormulaType = FormulaType;
        }

        public Indicator CreateEntity()
        {
            return new Indicator
            {
                ShortName = ShortName,
                Name = Name,
                InAnalysis = InAnalysis,
                FormulaType = FormulaType
            };
        }
    }
}