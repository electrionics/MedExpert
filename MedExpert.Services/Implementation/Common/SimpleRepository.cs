using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Domain;
using MedExpert.Services.Interfaces.Common;

namespace MedExpert.Services.Implementation.Common
{
    public class SimpleRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly MedExpertDataContext _dataContext;

        public SimpleRepository(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task DeleteBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {
            _dataContext.Set<TEntity>().RemoveRange(entities);
            await _dataContext.SaveChangesAsync();
        }

        public async Task InsertBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {
            await _dataContext.Set<TEntity>().AddRangeAsync(entities);
            await _dataContext.SaveChangesAsync();
        }

        public async Task UpdateBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {
            await _dataContext.SaveChangesAsync();
        }
    }
}
