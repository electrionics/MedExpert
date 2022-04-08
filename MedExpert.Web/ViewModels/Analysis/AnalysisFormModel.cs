using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class AnalysisFormModel
    {
        public int AnalysisId { get; set; }
        
        public ProfileModel Profile { get; set; }
        
        public List<IndicatorValueModel> Indicators { get; set; }
        
        public List<int> SpecialistIds { get; set; }
    }
}