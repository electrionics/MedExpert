using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IDeviationLevelService:IImportService<AnalysisDeviationLevel>
    {
        public Task<List<DeviationLevel>> GetAll();

        DeviationLevel Calculate(decimal refIntervalMin, decimal refIntervalMax, decimal value,
            IList<AnalysisDeviationLevel> deviationLevelsSorted);
    }
}