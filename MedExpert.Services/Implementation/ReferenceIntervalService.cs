﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Services.Interfaces;

namespace MedExpert.Services.Implementation
{
    public class ReferenceIntervalService: IReferenceIntervalService
    {
        private readonly MedExpertDataContext _dataContext;

        public ReferenceIntervalService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<ReferenceIntervalValues>> GetReferenceIntervalsByCriteria(Sex sex, decimal age)
        {
            return await _dataContext.Set<ReferenceIntervalValues>().Where(x =>
                    x.ApplyCriteria.Sex == sex &&
                    x.ApplyCriteria.AgeMin <= age &&
                    x.ApplyCriteria.AgeMax > age &&
                    x.Indicator.InAnalysis)
                .ToListAsync();
        }

        public async Task DeleteAllReferenceIntervalValues()
        {
            var toRemove = _dataContext.Set<ReferenceIntervalValues>();
            _dataContext.Set<ReferenceIntervalValues>().RemoveRange(toRemove);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAllReferenceIntervalApplyCriteria()
        {
            var toRemove = _dataContext.Set<ReferenceIntervalApplyCriteria>();
            _dataContext.Set<ReferenceIntervalApplyCriteria>().RemoveRange(toRemove);
            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateBulk(List<ReferenceIntervalApplyCriteria> indicators)
        {
            await _dataContext.SaveChangesAsync();
        }

        public async Task InsertBulk(List<ReferenceIntervalApplyCriteria> entities)
        {
            await _dataContext.Set<ReferenceIntervalApplyCriteria>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
        }

        public Task Insert(ReferenceIntervalApplyCriteria entity)
        {
            throw new System.NotImplementedException();
        }
    }
}