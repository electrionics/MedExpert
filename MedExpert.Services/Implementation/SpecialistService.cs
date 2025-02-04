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
    public class SpecialistService:ISpecialistService
    {
        private readonly MedExpertDataContext _dataContext;

        public SpecialistService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Specialist>> GetSpecialistsByCriteria(Sex sex)
        {
            return await _dataContext.Set<Specialist>()
                .Where(x => x.ApplyToSexOnly == null || x.ApplyToSexOnly == sex)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Specialist>> GetSpecialists()
        {
            return await _dataContext.Set<Specialist>().OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<Specialist> GetSpecialistById(int specialistId)
        {
            return await _dataContext.Set<Specialist>().FirstOrDefaultAsync(x => x.Id == specialistId);
        }

        public async Task CreateSpecialist(Specialist specialist)
        {
            await _dataContext.Set<Specialist>().AddAsync(specialist);
            await _dataContext.SaveChangesAsync();
        }
    }
}