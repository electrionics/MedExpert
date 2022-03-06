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