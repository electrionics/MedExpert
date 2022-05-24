using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class AnalysisSymptomIndicatorService:IAnalysisSymptomIndicatorService
    {
        private readonly MedExpertDataContext _dataContext;

        public AnalysisSymptomIndicatorService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task InsertBulk(List<AnalysisSymptomIndicator> entities)
        {
            await _dataContext.BulkInsertAsync(entities, options =>
            {
                options.IncludeGraph = false;
                options.BatchSize = 5000;
            });
        }
    }
}