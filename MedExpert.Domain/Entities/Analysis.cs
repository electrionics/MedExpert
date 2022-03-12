using System;
using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Domain.Entities
{
    public class Analysis
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public Sex Sex { get; set; }
        
        public int Age { get; set; }
        
        public DateTime CalculationTime { get; set; }
        
        public User User { get; set; }
        
        
        public List<AnalysisDeviationLevel> AnalysisDeviationLevels { get; set; }
        
        public List<AnalysisIndicator> AnalysisIndicators { get; set; }
        
        public List<AnalysisSymptom> AnalysisSymptoms { get; set; }
    }
}