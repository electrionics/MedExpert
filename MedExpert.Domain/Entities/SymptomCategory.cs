using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class SymptomCategory
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        
        public List<Symptom> Symptoms { get; set; }
    }
}