using System.Collections.Generic;

namespace MedExpert.Core.Helpers
{
    public class DoubleEqualityComparer:IEqualityComparer<double>
    {
        private readonly byte _accuracy;

        public DoubleEqualityComparer(byte accuracy)
        {
            _accuracy = accuracy;
        }
        
        public bool Equals(double x, double y)
        {
            return x.EqualTo(y, _accuracy);
        }

        public int GetHashCode(double obj)
        {
            return obj.RoundTo(_accuracy).GetHashCode();
        }
    }
}