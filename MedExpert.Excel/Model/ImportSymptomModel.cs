﻿using System.Collections.Generic;
using System.Linq;
using MedExpert.Domain.Entities;

namespace MedExpert.Excel.Model
{
    public class ImportSymptomModel
    {
        public string SymptomName { get; set; }
        
        public string SymptomDescription { get; set; }
        
        public string SymptomLevelStr { get; set; }
        
        public int SymptomLevel { get; set; }
        
        public Dictionary<string, DeviationLevelModel> DeviationLevels { get; set; }

        public ImportSymptomModel()
        {
            DeviationLevels = new Dictionary<string, DeviationLevelModel>();
        }

        public Symptom CreateEntity(Dictionary<string, DeviationLevel> deviationLevels, Dictionary<string, Indicator> indicators, int specialistId)
        {
            return new Symptom
            {
                SpecialistId = specialistId,
                Name = SymptomName,
                Comment = SymptomDescription,
                SymptomIndicatorDeviationLevels = DeviationLevels.Select(x =>
                {
                    if (deviationLevels.TryGetValue(x.Value.Alias ?? "", out var deviationLevel))
                    {
                        return new SymptomIndicatorDeviationLevel
                        {
                            Comment = x.Value.Description,
                            DeviationLevelId = deviationLevel.Id,
                            IndicatorId = indicators[x.Key].Id
                        };
                    }

                    return null;
                }).Where(x => x != null).ToList()
            };
        }
    }
}