using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class Symptom
    {
        public int Id { get; set; }
        
        public int? ParentSymptomId { get; set; }

        public int SpecialistId { get; set; }
        
        public string Name { get; set; }
        
        public string Comment { get; set; }
        
        public bool IsDeleted { get; set; }
        
        
        public Symptom Parent { get; set; }
        
        public Specialist Specialist { get; set; }
        
        
        public List<Symptom> Children { get; set; }
        
        public List<SymptomIndicatorDeviationLevel> SymptomIndicatorDeviationLevels { get; set; }
    }
}