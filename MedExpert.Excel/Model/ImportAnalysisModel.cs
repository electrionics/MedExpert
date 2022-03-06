using System;
using System.Collections.Generic;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;

namespace MedExpert.Excel.Model
{
    public class ImportAnalysisModel
    {
        public Sex? Sex { get; set; }
        
        public int? Age { get; set; }
        
        public Dictionary<string, string> OtherColumns { get; set; }
        
        public ImportAnalysisModel()
        {
            OtherColumns = new Dictionary<string, string>();
        }

        public Analysis CreateEntity()
        {
            return new()
            {
                Sex = Sex.Value,
                Age = Age.Value,
                CalculationTime = DateTime.Now,
                UserId = 1
            };
        }
    }
}