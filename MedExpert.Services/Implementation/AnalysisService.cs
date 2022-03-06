using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class AnalysisService : IAnalysisService
    {
        private readonly MedExpertDataContext _dataContext;

        public AnalysisService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public Task UpdateBulk(List<Analysis> entities)
        {
            throw new System.NotImplementedException();
        }

        public Task InsertBulk(List<Analysis> entities)
        {
            throw new System.NotImplementedException();
        }

        public async Task Insert(Analysis entity)
        {
            _dataContext.Set<Analysis>().Add(entity);
            await _dataContext.SaveChangesAsync();
        }
    }
}