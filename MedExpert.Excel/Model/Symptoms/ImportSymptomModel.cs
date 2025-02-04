﻿using System.Collections.Generic;
using System.Linq;

using MedExpert.Domain.Entities;

namespace MedExpert.Excel.Model.Symptoms
{
    public class ImportSymptomModel
    {
        public SymptomNameModel SymptomName { get; set; }
        
        public string SymptomDescription { get; set; }
        
        public string SymptomLevelStr { get; set; }
        
        public int SymptomLevel { get; set; }
        
        public Dictionary<string, DeviationLevelModel> DeviationLevels { get; set; }

        public ImportSymptomModel()
        {
            DeviationLevels = new Dictionary<string, DeviationLevelModel>();
        }

        public Symptom CreateEntity(Dictionary<string, DeviationLevel> deviationLevels, Dictionary<string, Indicator> indicators, int specialistId, int categoryId)
        {
            return new Symptom
            {
                SpecialistId = specialistId,
                CategoryId = categoryId,
                Name = SymptomName.Value,
                ApplyToSexOnly = SymptomName.Sex,
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
        
        
        // protected bool Equals(ImportSymptomModel other)
        // {
        //     return Equals(SymptomName, other.SymptomName) && SymptomLevelStr == other.SymptomLevelStr;
        // }
        //
        // public override bool Equals(object obj)
        // {
        //     if (ReferenceEquals(null, obj)) return false;
        //     if (ReferenceEquals(this, obj)) return true;
        //     if (obj.GetType() != this.GetType()) return false;
        //     return Equals((ImportSymptomModel) obj);
        // }
        //
        // public override int GetHashCode()
        // {
        //     unchecked
        //     {
        //         return ((SymptomName?.GetHashCode() ?? 0) * 397) ^ (SymptomLevelStr?.GetHashCode() ?? 0);
        //     }
        // }
    }
}