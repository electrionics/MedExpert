using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface ISymptomCategoryService
    {
        Task<bool> CategoryExists(int categoryId);
        Task<SymptomCategory> GetByName(string name);
        Task<List<SymptomCategory>> GetAll();
    }
}