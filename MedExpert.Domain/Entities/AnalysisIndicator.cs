namespace MedExpert.Domain.Entities
{
    public class AnalysisIndicator
    {
        public int AnalysisId { get; set; }
        
        public int IndicatorId { get; set; }
        
        public decimal Value { get; set; }
        
        public decimal ReferenceIntervalValueMin { get; set; }
        
        public decimal ReferenceIntervalValueMax { get; set; }
        
        public int DeviationLevelId { get; set; }
        
        public Analysis Analysis { get; set; }
        
        public Indicator Indicator { get; set; }
        
        public DeviationLevel DeviationLevel { get; set; }
    }
}