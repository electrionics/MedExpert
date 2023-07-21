using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MedExpert.Core;
using MedExpert.Core.Helpers;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using MedExpert.Services.Interfaces.Common;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class SymptomService:ISymptomService
    {
        private readonly MedExpertDataContext _dataContext;
        private readonly IRepository<Symptom> _repository;
        private readonly IRepository<SymptomIndicatorDeviationLevel> _sidlRepository;

        public SymptomService(MedExpertDataContext dataContext, IRepository<Symptom> repository, IRepository<SymptomIndicatorDeviationLevel> sidlRepository)
        {
            _dataContext = dataContext;
            _repository = repository;
            _sidlRepository = sidlRepository;
        }
        
        public async Task UpdateBulk(List<Symptom> entities)
        {
            await _dataContext.SaveChangesAsync();
            RefreshSymptomsCache();
        }

        public async Task InsertBulk(List<Symptom> entities)
        {
            await _repository.InsertBulk(entities, 2000, true);
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

            await _sidlRepository.DeleteBulk(entities, 200);
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
            
            foreach (var symptom in entitiesToMark)
            {
                symptom.IsDeleted = true;
            }

            await _repository.DeleteBulk(entitiesToRemove, 5000);
            await _repository.UpdateBulk(entitiesToMark, 5000);
        }

        #region Cache

        private static SemaphoreSlim _symptomCacheSemaphore = new SemaphoreSlim(1);
        private static IList<TreeItem<Symptom>> _symptomsCache;
        
        public async Task<IList<TreeItem<Symptom>>> GetSymptomsTree()
        {
            if (_symptomsCache == null)
            {
                try
                {
                    await _symptomCacheSemaphore.WaitAsync();

                    if (_symptomsCache == null)
                    {
                        var symptoms = await _dataContext.Set<Symptom>().AsNoTracking()
                            .Include(x => x.SymptomIndicatorDeviationLevels)
                            .ThenInclude(x => x.Indicator)
                            .Where(x => !x.IsDeleted)
                            .ToListAsync();

                        _symptomsCache = symptoms
                            .GenerateTree(x => x.Id, x => x.ParentSymptomId);
                    }
                }
                finally
                {
                    _symptomCacheSemaphore.Release();
                }
            }

            return _symptomsCache;
        }

        private static SemaphoreSlim _alwaysMatchedSymptomsSemaphore = new SemaphoreSlim(1);
        private static List<Symptom> _alwaysMatchedSymptoms;
        
        public async Task<List<Symptom>> GetAlwaysMatchedSymptoms()
        {
            if (_alwaysMatchedSymptoms == null)
            {
                try
                {
                    await _alwaysMatchedSymptomsSemaphore.WaitAsync();

                    if (_alwaysMatchedSymptoms == null)
                    {
                        _alwaysMatchedSymptoms = await _dataContext.Set<Symptom>().AsNoTracking()
                            .Include(x => x.Specialist)
                            .Where(x =>
                                !x.IsDeleted &&
                                !x.SymptomIndicatorDeviationLevels.Any(y => y.Indicator.InAnalysis))
                            .ToListAsync();
                    }
                }
                finally
                {
                    _alwaysMatchedSymptomsSemaphore.Release();
                }
            }

            return _alwaysMatchedSymptoms;
        }

        public void RefreshSymptomsCache()
        {
            _symptomsCache = null;
            _alwaysMatchedSymptoms = null;
        }

        #endregion
    }
}