using System;
using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Excel.Model.Analysis
{
    public class ImportAnalysisModel
    {
        public Sex? Sex { get; set; }
        
        public int? Age { get; set; }
        
        public DateTime? Date { get; set; }
        
        public Dictionary<string, string> OtherColumns { get; set; }
        
        public ImportAnalysisModel()
        {
            OtherColumns = new Dictionary<string, string>();
        }

        public Domain.Entities.Analysis CreateEntity()
        {
            return new()
            {
                Sex = Sex.Value,
                Age = Age.Value,
                Date = Date.Value,
                CalculationTime = DateTime.Now,
                UserId = 1
            };
        }
    }
}