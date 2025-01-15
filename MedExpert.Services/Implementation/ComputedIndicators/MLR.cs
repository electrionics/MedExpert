using System.Collections.Generic;

using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation.ComputedIndicators
{
    // ReSharper disable once InconsistentNaming
    public class MLR:IComputedIndicator
    {
        public string ShortName => "MLR";
        public List<string> DependentShortNames => new() {"M", "L"};
        public decimal Compute(Dictionary<string, decimal> indicatorValues)
        {
            var l = indicatorValues["L"];
            if (l == 0)
            {
                l = 0.01m;
            }
            return indicatorValues["M"] / l;
        }
    }
}