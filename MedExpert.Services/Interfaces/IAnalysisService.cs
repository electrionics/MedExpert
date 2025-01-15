using System.Collections.Generic;
using System.Threading.Tasks;

using MedExpert.Core;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IAnalysisService : IImportService<Analysis>
    {
        Task<IList<TreeItem<AnalysisSymptom>>> CalculateNewAnalysis(int analysisId, List<int> specialistIds);

        Task<IList<TreeItem<AnalysisSymptom>>> FetchCalculatedAnalysis(int analysisId, List<int> specialistIds, int categoryId);
        
        Task Update(Analysis analysis);
    }
}