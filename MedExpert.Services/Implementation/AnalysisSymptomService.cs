using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using MedExpert.Services.Interfaces.Common;

namespace MedExpert.Services.Implementation
{
    public class AnalysisSymptomService:IAnalysisSymptomService
    {
        private readonly MedExpertDataContext _dataContext;
        private readonly IRepository<AnalysisSymptom> _repository;

        public AnalysisSymptomService(MedExpertDataContext dataContext, IRepository<AnalysisSymptom> repository)
        {
            _dataContext = dataContext;
            _repository = repository;
        }

        public async Task InsertBulk(List<AnalysisSymptom> entities)
        {
            await _repository.InsertBulk(entities, 5000);
        }
    }
}