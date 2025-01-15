using System.Collections.Generic;

namespace MedExpert.Services.Interfaces
{
    public interface IComputedIndicator
    {
        string ShortName { get; }
        
        List<string> DependentShortNames { get; }

        decimal Compute(Dictionary<string, decimal> indicatorValues);
    }
}