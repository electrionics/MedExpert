using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class SymptomCategoryService:ISymptomCategoryService
    {
        private readonly MedExpertDataContext _dataContext;

        public SymptomCategoryService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task<bool> CategoryExists(int categoryId)
        {
            return await _dataContext.Set<SymptomCategory>().AnyAsync(x => x.Id == categoryId);
        }

        public async Task<SymptomCategory> GetByName(string name)
        {
            return await _dataContext.Set<SymptomCategory>().FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<List<SymptomCategory>> GetAll()
        {
            return await _dataContext.Set<SymptomCategory>().ToListAsync();
        }
    }
}