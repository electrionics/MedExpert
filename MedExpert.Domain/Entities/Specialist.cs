using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class Specialist
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        
        public List<Symptom> Symptoms { get; set; }
    }
}