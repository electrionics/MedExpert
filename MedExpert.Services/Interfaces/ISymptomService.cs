using System.Threading.Tasks;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface ISymptomService:IImportService<Symptom>
    {
        Task DeleteAllSymptomIndicatorDeviationLevels(int specialistId, int symptomCategoryId);
        Task TryDeleteAllSymptoms(int specialistId, int symptomCategoryId);
    }
}