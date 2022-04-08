using System.Collections.Generic;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation.ComputedIndicators
{
    // ReSharper disable once InconsistentNaming
    public class NLR:IComputedIndicator
    {
        public string ShortName => "NLR";
        public List<string> DependentShortNames => new() {"N", "L"};
        public decimal Compute(Dictionary<string, decimal> indicatorValues)
        {
            var l = indicatorValues["L"];
            if (l == 0)
            {
                l = 0.01m;
            }
            return indicatorValues["N"] / l;
        }
    }
}