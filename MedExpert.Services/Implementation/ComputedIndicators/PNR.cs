using System.Collections.Generic;

using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation.ComputedIndicators
{
    public class PNR : IComputedIndicator
    {
        public string ShortName => "PNR";
        public List<string> DependentShortNames => new() {"PLT", "N"};

        public decimal Compute(Dictionary<string, decimal> indicatorValues)
        {
            var l = indicatorValues["N"];
            if (l == 0)
            {
                l = 0.01m;
            }

            return indicatorValues["PLT"] / l;
        }
    }
}