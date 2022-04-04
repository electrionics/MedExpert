using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class AnalysisFormModel
    {
        public int AnalysisId { get; set; }
        
        public ProfileModel Profile { get; set; }
        
        public List<IndicatorModel> Indicators { get; set; }
        
        public List<int> SpecialistIds { get; set; }
    }
}