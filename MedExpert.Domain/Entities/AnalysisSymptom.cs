using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class AnalysisSymptom
    {
        public int AnalysisId { get; set; }
        
        public int SymptomId { get; set; }
        
        public decimal? Severity { get; set; }
        
        public Analysis Analysis { get; set; }
        
        public Symptom Symptom { get; set; }
        
        
        public HashSet<int> MatchedIndicatorIds { get; set; }
    }
}