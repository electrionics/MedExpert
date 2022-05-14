using System.Collections.Generic;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation.ComputedIndicators
{
    public class BLR:IComputedIndicator
    {
        public string ShortName => "BLR";
        public List<string> DependentShortNames => new() {"B", "L"};
        public decimal Compute(Dictionary<string, decimal> indicatorValues)
        {
            var l = indicatorValues["L"];
            if (l == 0)
            {
                l = 0.01m;
            }
            return indicatorValues["B"] / l;
        }
    }
}