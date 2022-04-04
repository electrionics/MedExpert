using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace MedExpert.Web.ViewModels.Analysis
{
    public class AnalysisResultModel
    {
        public int AnalysisId { get; set; }
        
        public List<MedicalStateModel> FoundMedicalStates { get; set; }
        
        public List<CommentModel> Comments { get; set; }
    }
}