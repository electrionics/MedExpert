namespace MedExpert.Domain.Entities
{
    public class AnalysisDeviationLevel
    {
        public int AnalysisId { get; set; }
        
        public int DeviationLevelId { get; set; }
        
        public int MinPercentFromCenter { get; set; }
        
        public int MaxPercentFromCenter { get; set; }
        
        
        public Analysis Analysis { get; set; }
        
        public DeviationLevel DeviationLevel { get; set; }
    }
}