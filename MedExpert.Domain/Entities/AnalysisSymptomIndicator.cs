namespace MedExpert.Domain.Entities
{
    public class AnalysisSymptomIndicator
    {
        public int AnalysisId { get; set; }
        
        public int SymptomId { get; set; }
        
        public int IndicatorId { get; set; }
        
        
        public Analysis Analysis { get; set; }
        
        public Symptom Symptom { get; set; }
        
        public Indicator Indicator { get; set; }
    }
}