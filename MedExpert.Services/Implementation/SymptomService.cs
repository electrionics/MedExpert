using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        }

        public async Task InsertBulk(List<Symptom> entities)
        {
            await _dataContext.Set<Symptom>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAllSymptomIndicatorDeviationLevels(int specialistId)
        {
            var entities = _dataContext.Set<SymptomIndicatorDeviationLevel>()
                .Where(x => x.Symptom.SpecialistId == specialistId);
            
            _dataContext.Set<SymptomIndicatorDeviationLevel>().RemoveRange(entities);

            await _dataContext.SaveChangesAsync();
        }

        public async Task TryDeleteAllSymptoms(int specialistId)
        {
            var entities = _dataContext.Set<Symptom>()
                .Include(x => x.Children)
                .Where(x => x.SpecialistId == specialistId && !x.IsDeleted)
                .ToList();

            var processEntities = entities.Where(x => !x.Children.Any()).ToList();
            var idsSet = processEntities.Select(x => x.Id).ToHashSet();
            
            var entitiesToMark = new List<Symptom>();
            var i = 0;
            while (i < processEntities.Count)
            {
                var processEntity = processEntities[i];
                try
                {
                    var parentId = processEntity.ParentSymptomId;
                    if (parentId != null && !idsSet.Contains(parentId.Value))
                    {
                        processEntities.Add(entities.First(x => x.Id == parentId));
                        idsSet.Add(parentId.Value);
                    }
                    
                    _dataContext.Set<Symptom>().Remove(processEntity);
                    await _dataContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    entitiesToMark.Add(processEntity);
                }
                
                i++;
            }

            foreach (var symptom in entitiesToMark)
            {
                symptom.IsDeleted = true;
            }

            if (entitiesToMark.Any())
            {
                await _dataContext.SaveChangesAsync();    
            }
        }
    }
}