using MedExpert.Domain.Enums;

namespace MedExpert.Excel.Model.Symptoms
{
    public class SymptomNameModel
    {
        public string Value { get; set; }
        
        public string SexStr { get; set; }
        
        public Sex? Sex { get; set; }
        
        
        // protected bool Equals(SymptomNameModel other)
        // {
        //     return Value == other.Value && Sex == other.Sex;
        // }
        //
        // public override bool Equals(object obj)
        // {
        //     if (ReferenceEquals(null, obj)) return false;
        //     if (ReferenceEquals(this, obj)) return true;
        //     if (obj.GetType() != this.GetType()) return false;
        //     return Equals((SymptomNameModel) obj);
        // }
        //
        // public override int GetHashCode()
        // {
        //     unchecked
        //     {
        //         return ((Value?.GetHashCode() ?? 0) * 397) ^ (Sex?.GetHashCode() ?? 0);
        //     }
        // }
    }
}