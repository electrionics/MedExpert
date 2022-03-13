using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Domain.Entities
{
    public class Specialist
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public Sex? ApplyToSexOnly { get; set; }
        
        
        public List<Symptom> Symptoms { get; set; }
    }
}