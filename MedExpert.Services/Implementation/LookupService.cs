using System.Threading.Tasks;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Services.Implementation
{
    public class LookupService:ILookupService
    {
        private readonly MedExpertDataContext _dataContext;

        public LookupService(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public Task<Lookup> GetByName(string name)
        {
            return _dataContext.Set<Lookup>().FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}