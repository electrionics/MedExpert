namespace MedExpert.Web.ViewModels.Analysis
{
    public class CommentModel
    {
        public int CommentId { get; set; } // SymptomId or DeviationLevelId, depends on type
        
        public string Name { get; set; }
        
        public CommentType Type { get; set; }

        public string Text { get; set; }
    }

    public enum CommentType
    {
        Illness = 1,
        Symptom = 2,
        MatchedIndicator = 3,
        RecommendedForAnalysisIndicator = 4
    }
}