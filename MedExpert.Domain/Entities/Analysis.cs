﻿using System;
using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class Analysis
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime CalculationTime { get; set; }
        
        public User User { get; set; }
        
        
        public List<AnalysisDeviationLevel> AnalysisDeviationLevels { get; set; }
        
        public List<AnalysisIndicator> AnalysisIndicators { get; set; }
    }
}