using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Domain;
using MedExpert.Services.Interfaces.Common;

namespace MedExpert.Services.Implementation.Common
{
    internal class OptimizedRepository<TEntity>:IRepository<TEntity>
        where TEntity : class
    {
        private readonly MedExpertDataContext _dataContext;

        public OptimizedRepository(MedExpertDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task InsertBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {
            await _dataContext.BulkInsertAsync(entities, options =>
            {
                options.IncludeGraph = includeGraph;
                options.BatchSize = batchSize;
            });
        }
        public async Task DeleteBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {
            await _dataContext.BulkDeleteAsync(entities, options =>
            {
                options.IncludeGraph = includeGraph;
                options.BatchSize = batchSize;
            });
        }
            
        public async Task UpdateBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false)
        {

            await _dataContext.BulkUpdateAsync(entities, options =>
            {
                options.IncludeGraph = includeGraph;
                options.BatchSize = batchSize;
            });
        }
    }
}
