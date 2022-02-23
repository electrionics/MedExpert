using System.Collections.Generic;
using System.Linq;

namespace NiceNumber.Helpers
{
    public class ByteArrayEqualityComparer:IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                var hashCode = (int)obj[0];
                hashCode = (hashCode * 397) ^ obj.Length / 2;
                hashCode = (hashCode * 397) ^ obj[obj.Length - 1];
                return hashCode;
            }
        }
            
        public static ByteArrayEqualityComparer Comparer => new ByteArrayEqualityComparer();
    }
}