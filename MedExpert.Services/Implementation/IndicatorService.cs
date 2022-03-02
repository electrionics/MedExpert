using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class IndicatorService:IIndicatorService
    {
        private readonly MedExpertDataContext _dataContext;

        public IndicatorService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Indicator>> GetIndicators(List<string> shortNames)
        {
            return await _dataContext.Set<Indicator>().Where(x => shortNames.Contains(x.ShortName)).ToListAsync();
        }

        public async Task UpdateBulk(List<Indicator> indicators)
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task InsertBulk(List<Indicator> indicators)
        {
            await _dataContext.Set<Indicator>().AddRangeAsync(indicators);
            await _dataContext.SaveChangesAsync();
        }
    }
}