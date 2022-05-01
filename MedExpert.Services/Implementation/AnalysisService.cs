using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.Core;
using MedExpert.Core.Helpers;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QueryableExtensions = MedExpert.Core.Helpers.QueryableExtensions;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace MedExpert.Services.Implementation
{
    public class AnalysisService : IAnalysisService
    {
        private readonly MedExpertDataContext _dataContext;

        private readonly ISymptomService _symptomService;

        public AnalysisService(MedExpertDataContext dataContext, ISymptomService symptomService)
        {
            _dataContext = dataContext;
            _symptomService = symptomService;
        }
        
        public Task UpdateBulk(List<Analysis> entities)
        {
            throw new NotImplementedException();
        }

        public Task InsertBulk(List<Analysis> entities)
        {
            throw new NotImplementedException();
        }

        public async Task Insert(Analysis entity)
        {
            _dataContext.Set<Analysis>().Add(entity);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<IList<TreeItem<AnalysisSymptom>>> CalculateNewAnalysis(int analysisId, List<int> specialistIds)
        {
            var analysis = await _dataContext.Set<Analysis>().FirstOrDefaultAsync(x => x.Id == analysisId);

            if (analysis.Calculated)
            {
                throw new InvalidDataException("Анализ уже рассчитан.");
            }

            Func<Tuple<SymptomIndicatorDeviationLevel, AnalysisIndicator>, bool> matcher = y =>
                y.Item2 != null &&
                y.Item1.DeviationLevelId * y.Item2.DeviationLevelId > 0 &&
                Math.Abs(y.Item1.DeviationLevelId) <= Math.Abs(y.Item2.DeviationLevelId);
            
            var matchedSymptoms = (await QueryableExtensions.LeftOuterJoin(
                    _dataContext.Set<SymptomIndicatorDeviationLevel>().Include(x => x.Indicator).Where(x =>
                            !x.Symptom.IsDeleted &&
                            specialistIds.Contains(x.Symptom.SpecialistId) &&
                            (x.Symptom.ApplyToSexOnly == null || x.Symptom.ApplyToSexOnly == analysis.Sex) &&
                            (x.Symptom.Specialist.ApplyToSexOnly == null || x.Symptom.Specialist.ApplyToSexOnly == analysis.Sex) &&
                            !x.Indicator.InAnalysis), 
                    _dataContext.Set<AnalysisIndicator>().Where(y => 
                        y.AnalysisId == analysisId && 
                        y.Indicator.InAnalysis), 
                    x => x.IndicatorId, 
                    y => y.IndicatorId, 
                    (si, ai) => new Tuple<SymptomIndicatorDeviationLevel, AnalysisIndicator>(si, ai))
                .GroupBy(x => x.Item1.SymptomId).Where(x => //TODO: from cache
                x.Count(matcher) >= Math.Min((x.Count() + 1) / 2, 3))
                .Select(x => new
                {
                    x.Key, 
                    MatchedIndicators = x
                        .Where(matcher)
                        .Select(y => y.Item1)
                        .ToList()
                })
                .ToListAsync()).ToDictionary(x => x.Key, x => x.MatchedIndicators);

            var symptomsTree = await _symptomService.GetSymptomsTree();

            var matchedSymptomsTree = symptomsTree.GetMatched(x => x.Id, matchedSymptoms.Keys.ToHashSet());

            var analysisIndicators = await _dataContext.Set<AnalysisIndicator>()
                .Include(x => x.Indicator)
                .Where(x => x.AnalysisId == analysisId)
                .ToListAsync();
            
            var analysisIndicatorDict = analysisIndicators
                .ToDictionary(x => x.IndicatorId, x => x.DeviationLevelId);

            var calculatedTree = matchedSymptomsTree.VisitAndConvert(x =>
                CalculateExpressiveness(x, analysisIndicatorDict, analysisId, matchedSymptoms));

            var filteredCalculatedTree =
                calculatedTree.GetMatched(x => x.Expressiveness is > 0.2m or null, new HashSet<bool> {true});
            
            return filteredCalculatedTree;
        }

        public async Task<IList<TreeItem<AnalysisSymptom>>> FetchCalculatedAnalysis(int analysisId, List<int> specialistIds, int categoryId)
        {
            var analysis = await _dataContext.Set<Analysis>().FirstOrDefaultAsync(x => x.Id == analysisId);
            
            if (!analysis.Calculated)
            {
                throw new InvalidDataException("Анализ еще не рассчитан.");
            }
            
            var filteredAnalysisSymptoms = _dataContext.Set<AnalysisSymptom>()
                .Include(x => x.Symptom)
                .ThenInclude(x => x.SymptomIndicatorDeviationLevels.Where(y => !y.Indicator.InAnalysis))
                .ThenInclude(x => x.Indicator)
                .Where(x =>
                    x.AnalysisId == analysisId &&
                    specialistIds.Contains(x.Symptom.SpecialistId) &&
                    x.Symptom.CategoryId == categoryId);
            var matchedIndicators = _dataContext.Set<AnalysisSymptomIndicator>()
                .Where(x => x.AnalysisId == analysisId);

            var filteredAnalysisSymptomsWithIndicators = await filteredAnalysisSymptoms.GroupJoin(matchedIndicators,
                (ansy) => new {ansy.AnalysisId, ansy.SymptomId},
                (ansyin) => new {ansyin.AnalysisId, ansyin.SymptomId}, (analysisSymptom, indicators) => new
                {
                    AnalysisSymptom = analysisSymptom,
                    MatchedIndicatorIds = indicators.Select(x => x.IndicatorId).ToHashSet()
                }).ToListAsync();

            foreach (var smi in filteredAnalysisSymptomsWithIndicators)
            {
                smi.AnalysisSymptom.MatchedIndicatorIds = smi.MatchedIndicatorIds;
            }

            var matchedAnalysisSymptoms = filteredAnalysisSymptomsWithIndicators
                .Select(x => x.AnalysisSymptom)
                .ToDictionary(x => x.SymptomId, x => x);
            
            var symptomsTree = await _symptomService.GetSymptomsTree();

            var mathcedSymptomIds = matchedAnalysisSymptoms.Keys.ToHashSet();
            var matchedSymptomsTree = symptomsTree.GetMatched(x => x.Id, mathcedSymptomIds);

            var result = matchedSymptomsTree.VisitAndConvert(x => matchedAnalysisSymptoms[x.Id]);

            return result;
        }
        
        public async Task Update(Analysis analysis)
        {
            _dataContext.Set<Analysis>().Update(analysis);
            await _dataContext.SaveChangesAsync();
        }

        private static readonly Dictionary<string, double> IndicatorWeightsDict = new()
        {
            {"nRBC", 3}, {"WBC", 3}, {"PLT", 3}, {"NLR", 3},
                
            {"NRBC", 2}, {"RET", 2}, {"HB", 2}, {"HCT", 2}, {"MCV", 2}, {"RDW", 2}, {"ESR", 2}, {"N", 2}, {"E", 2},
            {"B", 2}, {"M", 2}, {"L", 2}, {"MPV", 2}, {"PLR", 2}, {"SII", 2}
        };

        private static readonly Dictionary<int, double> AbsoluteDeviationLevelMeasure = new()
        {
            {0, 0}, {1, 1}, {2, 2}, {3, Math.Sqrt(13)}, {4, Math.Sqrt(27)}, {5, Math.Sqrt(40)}
        };

        private static readonly Dictionary<int, double> BaseExpressivenessForCountOfIndicators = new()
        {
            {1, 0.4}, {2, 0.5}, {3, 0.6}, {4, 0.6}, {5, 0.6}, {6, 0.6}, {7, 0.7}, {8, 0.7}, {9, 0.8}
        };

        private static AnalysisSymptom CalculateExpressiveness(Symptom symptom,
            IDictionary<int, int> analysisIndicatorDict, int analysisId,
            IReadOnlyDictionary<int, List<SymptomIndicatorDeviationLevel>> matchedIndicators)
        {
            var symptomIndicators = symptom.SymptomIndicatorDeviationLevels
                .Select(x => new {x.Indicator, x.DeviationLevelId})
                .ToList();

            double? expressiveness;

            if (symptomIndicators.Any())
            {
                var vectorMaxExpressiveness = new double[symptomIndicators.Count];
                var vectorMinExpressiveness = new double[symptomIndicators.Count];
                var vectorRequired = new double[symptomIndicators.Count];
                var vectorAnalysis = new double[symptomIndicators.Count];

                for (var i = 0; i < symptomIndicators.Count; i++)
                {
                    var symptomIndicator = symptomIndicators[i];

                    if (!analysisIndicatorDict.ContainsKey(symptomIndicator.Indicator.Id))
                    {
                        analysisIndicatorDict[symptomIndicator.Indicator.Id] = 0; //TODO: is it necessary?
                    }

                    vectorRequired[i] = AbsoluteDeviationLevelMeasure[Math.Abs(symptomIndicator.DeviationLevelId)]
                                            .MultiplySign(symptomIndicator.DeviationLevelId)
                                        * Math.Sqrt(
                                            IndicatorWeightsDict.GetValueOrDefault(symptomIndicator.Indicator.ShortName,
                                                1));
                    vectorAnalysis[i] =
                        AbsoluteDeviationLevelMeasure[Math.Abs(analysisIndicatorDict[symptomIndicator.Indicator.Id])]
                            .MultiplySign(analysisIndicatorDict[symptomIndicator.Indicator.Id])
                        * Math.Sqrt(IndicatorWeightsDict.GetValueOrDefault(symptomIndicator.Indicator.ShortName, 1));
                    vectorMaxExpressiveness[i] = AbsoluteDeviationLevelMeasure[5]
                                                     .MultiplySign(symptomIndicator.DeviationLevelId)
                                                 * Math.Sqrt(
                                                     IndicatorWeightsDict.GetValueOrDefault(
                                                         symptomIndicator.Indicator.ShortName, 1));
                    vectorMinExpressiveness[i] = AbsoluteDeviationLevelMeasure[5]
                                                     .MultiplySign(symptomIndicator.DeviationLevelId * -1)
                                                 * Math.Sqrt(
                                                     IndicatorWeightsDict.GetValueOrDefault(
                                                         symptomIndicator.Indicator.ShortName, 1));
                }

                var vectorMaxExpressivenessProjected = vectorRequired.Project(vectorMaxExpressiveness);
                var vectorMinExpressivenessProjected = vectorRequired.Project(vectorMinExpressiveness);
                var vectorAnalysisProjected = vectorRequired.Project(vectorAnalysis);

                var baseExpressiveness =
                    BaseExpressivenessForCountOfIndicators.GetValueOrDefault(symptomIndicators.Count, 0.8);

                if (vectorAnalysisProjected.Subtract(vectorMaxExpressivenessProjected).Distance() >
                    vectorAnalysisProjected.Subtract(vectorMinExpressivenessProjected).Distance())
                {
                    var baseDistance = vectorMaxExpressivenessProjected.Distance() - vectorRequired.Distance();
                    var resultDistance =
                        vectorMaxExpressivenessProjected.Distance() - vectorAnalysisProjected.Distance();

                    expressiveness = baseExpressiveness +
                                     (1 - baseExpressiveness) * (baseDistance - resultDistance) / baseDistance;
                }
                else
                {
                    var baseDistance = vectorMinExpressivenessProjected.Distance() - vectorRequired.Distance();
                    var resultDistance =
                        vectorMinExpressivenessProjected.Distance() - vectorAnalysisProjected.Distance();

                    expressiveness = baseExpressiveness * (baseDistance - resultDistance) / baseDistance;
                }
            }
            else
            {
                expressiveness = null;
            }

            return new AnalysisSymptom 
            {
                Symptom = symptom,
                SymptomId = symptom.Id, 
                Expressiveness = (decimal?) expressiveness?.RoundTo(3), 
                AnalysisId = analysisId,
                MatchedIndicatorIds = matchedIndicators[symptom.Id]
                    .Select(x => x.IndicatorId)
                    .ToHashSet()
            };
        }
    }
}