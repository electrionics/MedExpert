namespace MedExpert.Domain.Entities
{
    public class SymptomIndicatorDeviationLevel
    {
        public int SymptomId { get; set; }
        
        public int IndicatorId { get; set; }
        
        public int DeviationLevelId { get; set; }
        
        public string Comment { get; set; }
        
        
        public Symptom Symptom { get; set; }
        
        public Indicator Indicator { get; set; }
        
        public DeviationLevel DeviationLevel { get; set; }
    }
}