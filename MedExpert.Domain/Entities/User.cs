using System.Collections.Generic;

namespace MedExpert.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public string Hash { get; set; }
        
        public string Salt { get; set; }
        
        
        public List<Analysis> Analyses { get; set; }
    }
}