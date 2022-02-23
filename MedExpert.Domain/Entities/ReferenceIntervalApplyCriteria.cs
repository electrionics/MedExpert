using System.Collections.Generic;
using MedExpert.Domain.Enums;

namespace MedExpert.Domain.Entities
{
    public class ReferenceIntervalApplyCriteria
    {
        public int Id { get; set; }
        
        public Sex Sex { get; set; }
        
        public decimal AgeMin { get; set; }
        
        public decimal AgeMax { get; set; }
        
        
        public List<ReferenceIntervalValues> ReferenceIntervalValues { get; set; }
    }
}