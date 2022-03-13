using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Domain.Entities
{
    public class Symptom
    {
        public int Id { get; set; }
        
        public int? ParentSymptomId { get; set; }

        public int SpecialistId { get; set; }
        
        public int CategoryId { get; set; }
        
        public Sex? ApplyToSexOnly { get; set; }
        
        public string Name { get; set; }
        
        public string Comment { get; set; }
        
        public bool IsDeleted { get; set; }
        
        
        public Symptom Parent { get; set; }
        
        public Specialist Specialist { get; set; }
        
        public SymptomCategory Category { get; set; }
        
        
        public List<Symptom> Children { get; set; }
        
        public List<SymptomIndicatorDeviationLevel> SymptomIndicatorDeviationLevels { get; set; }
        
        public List<AnalysisSymptom> AnalysisSymptoms { get; set; }
    }
}