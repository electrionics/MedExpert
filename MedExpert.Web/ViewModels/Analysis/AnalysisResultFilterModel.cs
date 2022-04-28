using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class AnalysisResultFilterModel
    {
        public int AnalysisId { get; set; }
        
        public MedicalStateFilter Filter { get; set; }
        
        public List<int> SpecialistIds { get; set; }
    }

    public enum MedicalStateFilter
    {
        Diseases = 1,
        CommonTreatment = 2,
        SpecialTreatment = 3,
        CommonAnalysis = 4,
        SpecialAnalysis = 5
    }
}