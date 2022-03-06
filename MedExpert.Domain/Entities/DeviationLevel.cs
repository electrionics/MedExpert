using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class DeviationLevel
    {
        public int Id { get; set; }
        
        public string Alias { get; set; }
        
        public int? MinPercentFromCenter { get; set; }
        
        public int? MaxPercentFromCenter { get; set; }
        
        
        public List<AnalysisDeviationLevel> AnalysisDeviationLevels { get; set; }
        
        public List<AnalysisIndicator> AnalysisIndicators { get; set; }
        
        public List<SymptomIndicatorDeviationLevel> SymptomIndicatorDeviationLevels { get; set; }
    }
}