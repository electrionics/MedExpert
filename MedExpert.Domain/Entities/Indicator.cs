using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Domain.Entities
{
    public class Indicator
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string ShortName { get; set; }
        
        public bool InAnalysis { get; set; }
        
        public FormulaType FormulaType { get; set; }
        
        
        public List<AnalysisIndicator> AnalysisIndicators { get; set; }
        
        public List<ReferenceIntervalValues> ReferenceIntervalValues { get; set; }
        
        public List<SymptomIndicatorDeviationLevel> SymptomIndicatorDeviationLevels { get; set; }
    }
}