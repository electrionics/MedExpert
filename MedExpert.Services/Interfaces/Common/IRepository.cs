using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedExpert.Services.Interfaces.Common
{
    // goal of this interface is to abstract ЯZ.EntityFramework operations
    public interface IRepository<TEntity>
        where TEntity : class
    {
        Task InsertBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false);
        Task DeleteBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false);
        Task UpdateBulk(IEnumerable<TEntity> entities, int batchSize, bool includeGraph = false);
    }
}
