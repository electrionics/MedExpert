using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using MedExpert.Services.Interfaces.Common;

namespace MedExpert.Services.Implementation
{
    public class AnalysisSymptomIndicatorService:IAnalysisSymptomIndicatorService
    {
        private readonly MedExpertDataContext _dataContext;
        private readonly IRepository<AnalysisSymptomIndicator> _repository;

        public AnalysisSymptomIndicatorService(MedExpertDataContext dataContext, IRepository<AnalysisSymptomIndicator> repository)
        {
            _dataContext = dataContext;
            _repository = repository;
        }
        
        public async Task InsertBulk(List<AnalysisSymptomIndicator> entities)
        {
            await _repository.InsertBulk(entities, 5000);
        }
    }
}