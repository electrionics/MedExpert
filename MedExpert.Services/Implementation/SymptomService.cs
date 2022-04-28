using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.Core;
using MedExpert.Core.Helpers;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class SymptomService:ISymptomService
    {
        private readonly MedExpertDataContext _dataContext;

        public SymptomService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task UpdateBulk(List<Symptom> entities)
        {
            await _dataContext.SaveChangesAsync();
            RefreshSymptomsCache();
        }

        public async Task InsertBulk(List<Symptom> entities)
        {
            await _dataContext.Set<Symptom>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
            RefreshSymptomsCache();
        }

        public Task Insert(Symptom entity)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAllSymptomIndicatorDeviationLevels(int specialistId, int symptomCategoryId)
        {
            var entities = _dataContext.Set<SymptomIndicatorDeviationLevel>()
                .Where(x => x.Symptom.SpecialistId == specialistId && x.Symptom.CategoryId == symptomCategoryId);
            
            _dataContext.Set<SymptomIndicatorDeviationLevel>().RemoveRange(entities);

            await _dataContext.SaveChangesAsync();
        }

        public async Task TryDeleteAllSymptoms(int specialistId, int symptomCategoryId)
        {
            var entities = await _dataContext.Set<Symptom>()
                .Include(x => x.Children)
                .Where(x => x.SpecialistId == specialistId && x.CategoryId == symptomCategoryId && !x.IsDeleted)
                .Select(x => new Tuple<Symptom, bool>(x, x.AnalysisSymptoms.Any() || x.AnalysisSymptomIndicators.Any()))
                .ToListAsync();

            var processEntities = entities.Where(x => !x.Item1.Children.Any()).ToList();
            var idsSet = processEntities.Select(x => x.Item1.Id).ToHashSet();
            
            var entitiesToMark = new List<Symptom>();
            var entitiesToRemove = new List<Symptom>();
            var i = 0;
            while (i < processEntities.Count)
            {
                var (processEntity, dependenciesExists) = processEntities[i];
                var parentId = processEntity.ParentSymptomId;
                if (parentId != null && !idsSet.Contains(parentId.Value))
                {
                    processEntities.Add(entities.First(x => x.Item1.Id == parentId));
                    idsSet.Add(parentId.Value);
                }

                if (dependenciesExists)
                {
                    entitiesToMark.Add(processEntity);
                }
                else
                {
                    entitiesToRemove.Add(processEntity);
                }

                i++;
            }

            _dataContext.Set<Symptom>().RemoveRange(entitiesToRemove);
            
            foreach (var symptom in entitiesToMark)
            {
                symptom.IsDeleted = true;
            }

            await _dataContext.SaveChangesAsync();
        }

        private static IList<TreeItem<Symptom>> _symptomsCache;
        
        public async Task<IList<TreeItem<Symptom>>> GetSymptomsTree()
        {
            if (_symptomsCache != null) return _symptomsCache;
            
            var symptoms = await _dataContext.Set<Symptom>()
                .Include(x => x.SymptomIndicatorDeviationLevels)
                .ThenInclude(x => x.Indicator)
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            _symptomsCache = symptoms
                .GenerateTree(x => x.Id, x => x.ParentSymptomId);

            return _symptomsCache;
        }

        public void RefreshSymptomsCache()
        {
            _symptomsCache = null;
        }
    }
}