using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class DeviationLevelService:IDeviationLevelService
    {
        private readonly MedExpertDataContext _dataContext;

        public DeviationLevelService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task<List<DeviationLevel>> GetAll()
        {
            return await _dataContext.Set<DeviationLevel>()
                .ToListAsync();
        }
        
        public DeviationLevel Calculate(decimal refIntervalMin, decimal refIntervalMax, decimal value,
            IList<AnalysisDeviationLevel> deviationLevelsSorted)
        {
            var refCenter = (refIntervalMin + refIntervalMax) / 2;
            var refLength = refIntervalMax - refIntervalMin;

            if (value < refCenter)
            {
                var percentFromCenter = (refCenter - value) / refLength * 100;
                var deviationLevels = deviationLevelsSorted.Where(x =>
                    x.DeviationLevelId <= 0).ToArray();

                for (var i = deviationLevels.Length - 1; i > 0; i--)
                {
                    if (deviationLevels[i].MinPercentFromCenter > percentFromCenter) // TODO: or >=
                    {
                        return deviationLevels[i].DeviationLevel;
                    }
                }

                return deviationLevels[0].DeviationLevel;
            }
            else
            {
                var percentFromCenter = (value - refCenter) / refLength * 100;
                var deviationLevels = deviationLevelsSorted.Where(x =>
                    x.DeviationLevelId >= 0).ToArray();
                
                for (var i = 0; i < deviationLevels.Length - 1; i++)
                {
                    if (deviationLevels[i].MaxPercentFromCenter > percentFromCenter) // TODO: or >=
                    {
                        return deviationLevels[i].DeviationLevel;
                    }
                }

                return deviationLevels[^1].DeviationLevel;
            }
        }

        public Task UpdateBulk(List<AnalysisDeviationLevel> entities)
        {
            throw new System.NotImplementedException();
        }

        public async Task InsertBulk(List<AnalysisDeviationLevel> entities)
        {
            await _dataContext.Set<AnalysisDeviationLevel>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
        }

        public Task Insert(AnalysisDeviationLevel entity)
        {
            throw new System.NotImplementedException();
        }
    }
}