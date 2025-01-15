using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedExpert.Services.Interfaces
{
    public interface IImportService<TImportEntity>
    {
        Task UpdateBulk(List<TImportEntity> entities);
        Task InsertBulk(List<TImportEntity> entities);
        Task Insert(TImportEntity entity);
    }
}