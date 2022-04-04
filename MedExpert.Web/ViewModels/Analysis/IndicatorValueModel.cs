namespace MedExpert.Web.ViewModels.Analysis
{
    public class IndicatorValueModel:IndicatorModel
    {
        public decimal? Value { get; set; }
        
        public decimal? ReferenceIntervalMin { get; set; }
        
        public decimal? ReferenceIntervalMax { get; set; }
    }
}