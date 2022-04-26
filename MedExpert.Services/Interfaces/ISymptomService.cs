using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Core;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface ISymptomService:IImportService<Symptom>
    {
        Task<IList<TreeItem<Symptom>>> GetSymptomsTree();
        void RefreshSymptomsCache();
        Task DeleteAllSymptomIndicatorDeviationLevels(int specialistId, int symptomCategoryId);
        Task TryDeleteAllSymptoms(int specialistId, int symptomCategoryId);
    }
}