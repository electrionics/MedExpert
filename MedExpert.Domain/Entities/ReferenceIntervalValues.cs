namespace MedExpert.Domain.Entities
{
    public class ReferenceIntervalValues
    {
        public int Id { get; set; }
        
        public int IndicatorId { get; set; }
        
        public int ApplyCriteriaId { get; set; }
        
        public decimal ValueMin { get; set; }
        
        public decimal ValueMax { get; set; }
        
        
        public Indicator Indicator { get; set; }
        
        public ReferenceIntervalApplyCriteria ApplyCriteria { get; set; }
    }
}