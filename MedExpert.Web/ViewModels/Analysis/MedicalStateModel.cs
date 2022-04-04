using System.Collections.Generic;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class MedicalStateModel
    {
        public int SymptomId { get; set; }
        
        public string Name { get; set; }
        
        public List<MedicalStateModel> ChildSymptoms { get; set;  }
        
        public decimal Match { get; set; }
        
        public decimal Expressiveness { get; set; }
        
        public List<IndicatorModel> RecommendedAnalyses { get; set; } // can present at each tree node at any level 
    }
}