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
    }
}