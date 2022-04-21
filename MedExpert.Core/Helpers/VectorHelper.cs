using System;
using System.Linq;

namespace MedExpert.Core.Helpers
{
    public static class VectorHelper
    {
        public static double[] Project(this double[] @base, double[] toProject)
        {
            var result = new double[@base.Length];
            
            var denominator = .0;
            var numerator = .0;
            
            for (var i = 0; i < @base.Length; i++)
            {
                numerator += @base[i] * toProject[i];
                denominator += Math.Pow(toProject[i], 2);
            }
            for (var i = 0; i < @base.Length; i++)
            {
                result[i] = toProject[i] * numerator / denominator;
            }

            return result;
        }

        public static double[] Subtract(this double[] @base, double[] toSubtract)
        {
            var result = new double[@base.Length];
            for (var i = 0; i < @base.Length; i++)
            {
                result[i] = @base[i] - toSubtract[i];
            }

            return result;
        }

        public static double Distance(this double[] vector)
        {
            return Math.Sqrt(vector.Sum(x => Math.Pow(x, 2)));
        }
    }
}