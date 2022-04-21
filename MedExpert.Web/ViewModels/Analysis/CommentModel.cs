namespace MedExpert.Web.ViewModels.Analysis
{
    public class CommentModel
    {
        public int SymptomId { get; set; }
        
        public int SpecialistId { get; set; }

        public string Name { get; set; }
        
        public CommentType Type { get; set; }

        public string Text { get; set; }
    }

    public enum CommentType
    {
        Symptom = 1,
        MatchedIndicator = 2,
        RecommendedForAnalysisIndicator = 3
    }
}