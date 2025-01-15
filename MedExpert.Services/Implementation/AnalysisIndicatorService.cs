using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class AnalysisIndicatorService : IAnalysisIndicatorService
    {
        private readonly MedExpertDataContext _dataContext;

        public AnalysisIndicatorService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public Task UpdateBulk(List<AnalysisIndicator> entities)
        {
            throw new System.NotImplementedException();
        }

        public async Task InsertBulk(List<AnalysisIndicator> entities)
        {
            await _dataContext.Set<AnalysisIndicator>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
        }

        public Task Insert(AnalysisIndicator entity)
        {
            throw new System.NotImplementedException();
        }
    }
}