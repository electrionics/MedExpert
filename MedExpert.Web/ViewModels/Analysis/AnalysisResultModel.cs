using System.Collections.Generic;
using MedExpert.Core;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class AnalysisResultModel
    {
        public int AnalysisId { get; set; }
        
        public IList<TreeItem<MedicalStateModel>> FoundMedicalStates { get; set; }
        
        public List<CommentModel> Comments { get; set; }
    }
}