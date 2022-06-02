using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class AnalysisSymptomService:IAnalysisSymptomService
    {
        private readonly MedExpertDataContext _dataContext;

        public AnalysisSymptomService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task InsertBulk(List<AnalysisSymptom> entities)
        {
            await _dataContext.BulkInsertAsync(entities, options => 
            {
                options.IncludeGraph = false;
                options.BatchSize = 5000;
            });
        }
    }
}