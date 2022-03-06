using System.Collections.Generic;
using System.Linq;
using MedExpert.Domain.Entities;

namespace MedExpert.Excel.Model
{
    public class ImportAnalysisDeviationLevelModel
    {
        public string Alias { get; set; }
        
        public int? MinPercentFromCenter { get; set; }
        
        public int? MaxPercentFromCenter { get; set; }
        
        
        public Dictionary<string, string> OtherColumns { get; set; }
        
        public int? DeviationLevelId { get; set; }
        
        public List<string> AllowedAliases { get; set; }
        
        public List<int> AllowedDeviationLevelIds { get; set; }
        
        public ImportAnalysisDeviationLevelModel()
        {
            OtherColumns = new Dictionary<string, string>();
        }
        
        public AnalysisDeviationLevel CreateEntity(List<DeviationLevel> deviationLevels)
        {
            return new()
            {
                DeviationLevel = deviationLevels.First(x => x.Id == DeviationLevelId.Value),
                MinPercentFromCenter = MinPercentFromCenter,
                MaxPercentFromCenter = MaxPercentFromCenter
            };
        }
    }
}