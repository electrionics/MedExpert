using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedExpert.Core;
using MedExpert.Domain.Entities;

namespace MedExpert.Services.Interfaces
{
    public interface IAnalysisService : IImportService<Analysis>
    {
        Task<Tuple<Analysis, IList<TreeItem<AnalysisSymptom>>>> CalculateNewAnalysis(int analysisId, List<int> specialistIds);
        Task Update(Analysis analysis);
    }
}