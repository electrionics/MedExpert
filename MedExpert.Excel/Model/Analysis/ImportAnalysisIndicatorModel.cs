using System.Collections.Generic;

using MedExpert.Domain.Entities;
using MedExpert.Excel.Model.Common;

namespace MedExpert.Excel.Model.Analysis
{
    public class ImportAnalysisIndicatorModel
    {
        public string Indicator { get; set; }
        
        public decimal? Value { get; set; }
        
        public IntervalModel ReferenceInterval { get; set; }

        
        public Dictionary<string, string> OtherColumns { get; set; }
        
        public int? IndicatorId { get; set; }

        public bool Calculated { get; set; }
        
        public List<string> AllowedIndicatorShortNames { get; set; }

        public ImportAnalysisIndicatorModel()
        {
            OtherColumns = new Dictionary<string, string>();
        }

        public AnalysisIndicator CreateEntity()
        {
            return new()
            {
                Value = Value.Value, //TODO: calculate if FormulaType not null
                ReferenceIntervalValueMin = ReferenceInterval?.ValueMin,
                ReferenceIntervalValueMax = ReferenceInterval?.ValueMax,
                IndicatorId = IndicatorId.Value, 
                DeviationLevelId = 0 //TODO: calculate
            };
        }
    }
}