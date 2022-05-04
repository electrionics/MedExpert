using System;

namespace MedExpert.Core.Helpers
{
    public static class NumberExtensions
    {
        public static bool EqualTo(this double first, double second, byte accuracy)
        {
            return Math.Abs(first - second) <= Math.Pow(0.1 , accuracy);
        }

        public static double RoundTo(this double value, double accuracy)
        {
            var multiplier = Math.Pow(10, accuracy);
            return Math.Round(value * multiplier) / multiplier;
        }

        public static double MultiplySign(this double value, double signValue)
        {
            return value * (signValue < 0 ? -1 : 1);
        }
    }
}