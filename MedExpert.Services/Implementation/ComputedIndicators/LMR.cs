using System.Collections.Generic;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation.ComputedIndicators
{
    // ReSharper disable once InconsistentNaming
    public class LMR:IComputedIndicator
    {
        public string ShortName => "LMR";
        public List<string> DependentShortNames => new() {"L", "M"};
        public decimal Compute(Dictionary<string, decimal> indicatorValues)
        {
            var m = indicatorValues["M"];
            if (m == 0)
            {
                m = 0.01m;
            }
            return indicatorValues["L"] / m;
        }
    }
}