using System.Collections.Generic;
using System.Linq;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Excel.Model.Common;

namespace MedExpert.Excel.Model.ReferenceIntervals
{
    public class ImportReferenceIntervalModel
    {
        public Sex Sex { get; set; }
        
        public IntervalModel AgeInterval { get; set; }
        
        public Dictionary<string, IntervalModel> Values { get; set; }

        public ImportReferenceIntervalModel()
        {
            Values = new Dictionary<string, IntervalModel>();
        }

        public ReferenceIntervalApplyCriteria CreateEntity(Dictionary<string, Indicator> indicatorsDict)
        {
            return new ReferenceIntervalApplyCriteria
            {
                Sex = Sex,
                AgeMin = AgeInterval.ValueMin,
                AgeMax = AgeInterval.ValueMax,
                ReferenceIntervalValues = Values.Select(x => new ReferenceIntervalValues
                {
                    ValueMin = x.Value.ValueMin,
                    ValueMax = x.Value.ValueMax,
                    IndicatorId = indicatorsDict[x.Key].Id
                }).ToList()
            };
        }
    }
}