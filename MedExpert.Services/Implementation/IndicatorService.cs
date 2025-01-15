using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class IndicatorService:IIndicatorService
    {
        private readonly MedExpertDataContext _dataContext;

        private readonly ISymptomService _symptomService;

        public IndicatorService(MedExpertDataContext dataContext, ISymptomService symptomService)
        {
            _dataContext = dataContext;
            _symptomService = symptomService;
        }

        public async Task<List<Indicator>> GetIndicators(List<string> shortNames)
        {
            return await _dataContext.Set<Indicator>().Where(x => shortNames.Contains(x.ShortName)).ToListAsync();
        }

        public async Task<List<Indicator>> GetAnalysisIndicators()
        {
            return await _dataContext.Set<Indicator>().Where(x => x.InAnalysis).OrderBy(x => x.Sort).ToListAsync();
        }

        public async Task<List<string>> GetShortNamesNotExists(List<string> shortNames)
        {
            var existingShortNames = await _dataContext.Set<Indicator>()
                .Select(x => x.ShortName)
                .Where(x => shortNames.Contains(x))
                .Distinct()
                .ToListAsync();

            return shortNames.Where(x => !existingShortNames.Contains(x)).ToList();
        }

        public async Task UpdateBulk(List<Indicator> indicators)
        {
            await _dataContext.SaveChangesAsync();
            _symptomService.RefreshSymptomsCache();
        }

        public async Task InsertBulk(List<Indicator> indicators)
        {
            await _dataContext.Set<Indicator>().AddRangeAsync(indicators);
            await _dataContext.SaveChangesAsync();
            _symptomService.RefreshSymptomsCache();
        }

        public Task Insert(Indicator entity)
        {
            throw new NotImplementedException();
        }
    }
}