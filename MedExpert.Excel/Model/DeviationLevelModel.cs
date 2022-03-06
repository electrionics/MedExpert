using System.Collections.Generic;

namespace MedExpert.Excel.Model
{
    public class DeviationLevelModel
    {
        public string Alias { get; set; }

        public string Description { get; set; }
        
        public HashSet<string> AllowedAliases { get; set; }
    }
}