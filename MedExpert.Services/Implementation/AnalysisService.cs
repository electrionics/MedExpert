using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MedExpert.Core;
using MedExpert.Core.Helpers;
using MedExpert.Domain;
using MedExpert.Domain.Entities;
using MedExpert.Services.Interfaces;

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

            const int maxSidlAndAlnalysisLevelDifferenceOnIndicator = 5;

            Func<SymptomIndicatorDeviationLevel, bool> matcher = y =>
                y.Indicator.AnalysisIndicators.Any() &&
                y.DeviationLevelId * y.Indicator.AnalysisIndicators.First().DeviationLevelId > 0 &&
                Math.Abs(y.DeviationLevelId) <= Math.Abs(y.Indicator.AnalysisIndicators.First().DeviationLevelId);

            Func<SymptomIndicatorDeviationLevel, bool> highLevel = y =>
                Math.Abs(y.DeviationLevelId) >= 3;
            
            Func<SymptomIndicatorDeviationLevel, bool> highLevelMatcher = y =>
                matcher.Invoke(y) && highLevel.Invoke(y);
            
            Func<SymptomIndicatorDeviationLevel, bool> lowLevelMatcher = y =>
                matcher.Invoke(y) && Math.Abs(y.DeviationLevelId) < 3;

            Func<SymptomIndicatorDeviationLevel, bool> unmatcher = y =>
                y.Indicator.AnalysisIndicators.Any() &&
                Math.Abs(y.DeviationLevelId - y.Indicator.AnalysisIndicators.First().DeviationLevelId) >
                    maxSidlAndAlnalysisLevelDifferenceOnIndicator;

            var sidls = await _dataContext.Set<SymptomIndicatorDeviationLevel>().AsNoTracking()
                .Include(x => x.Indicator)
                .ThenInclude(x => x.AnalysisIndicators.Where(y => y.AnalysisId == analysisId))
                .In(specialistIds, x => x.Symptom.SpecialistId)
                .Where(x =>
                    (x.Symptom.ApplyToSexOnly == null || 
                     x.Symptom.ApplyToSexOnly == analysis.Sex) &&
                    (x.Symptom.Specialist.ApplyToSexOnly == null ||
                     x.Symptom.Specialist.ApplyToSexOnly == analysis.Sex) &&
                    !x.Symptom.IsDeleted &&
                    x.Indicator.InAnalysis)
                .ToListAsync();

            var alwaysMatchedSymptoms = (await _symptomService.GetAlwaysMatchedSymptoms())
                .Where(x =>
                    specialistIds.Contains(x.SpecialistId) &&
                    (x.ApplyToSexOnly == null || 
                     x.ApplyToSexOnly == analysis.Sex) &&
                    (x.Specialist.ApplyToSexOnly == null ||
                     x.Specialist.ApplyToSexOnly == analysis.Sex))
                .ToList();
            
            var matchedSymptoms = sidls
                .GroupBy(x => x.SymptomId)
                .Where(x =>
                    !x.Any(unmatcher) &&
                    x.Count(highLevelMatcher) + x.Count(lowLevelMatcher) >= Math.Min((x.Count() + 1) / 2, 3) &&
                    x.Count(highLevel) == x.Count(highLevelMatcher))
                .Select(x => new
                {
                    x.Key, 
                    MatchedIndicators = x
                        .Where(matcher)
                        .ToList()
                }).ToDictionary(x => x.Key, x => x.MatchedIndicators);
            
            foreach (var matchedSymptom in alwaysMatchedSymptoms)
            {
                matchedSymptoms.Add(matchedSymptom.Id, new List<SymptomIndicatorDeviationLevel>());
            }

            var symptomsTree = await _symptomService.GetSymptomsTree();

            var matchedSymptomsTree = symptomsTree.GetMatched(x => x.Id, matchedSymptoms.Keys.ToHashSet());

            var analysisIndicators = await _dataContext.Set<AnalysisIndicator>()
                .Include(x => x.Indicator)
                .Where(x => x.AnalysisId == analysisId)
                .ToListAsync();
            
            var analysisIndicatorDict = analysisIndicators
                .ToDictionary(x => x.IndicatorId, x => x.DeviationLevelId);

            var calculatedTree = matchedSymptomsTree.VisitAndConvert(x =>
                CalculateSeverity(x, analysisIndicatorDict, analysisId, matchedSymptoms));

            var filteredCalculatedTree = calculatedTree
                .GetMatched(x => x.Severity is null or > 0.2m, new HashSet<bool> {true})
                .GetMatchedBranch(x => x.Any(y => y.Severity is not null), new HashSet<bool?> {true})
                .VisitAndConvert(CalculateCombinedSeverity);
            
            return filteredCalculatedTree;
        }

        public async Task<IList<TreeItem<AnalysisSymptom>>> FetchCalculatedAnalysis(int analysisId, List<int> specialistIds, int categoryId)
        {
            var analysis = await _dataContext.Set<Analysis>().FirstOrDefaultAsync(x => x.Id == analysisId);
            
            if (!analysis.Calculated)
            {
                throw new InvalidDataException("Анализ еще не рассчитан.");
            }
            
            var filteredAnalysisSymptoms = await _dataContext.Set<AnalysisSymptom>().AsNoTracking()
                .Include(x => x.Symptom)
                .ThenInclude(x => x.SymptomIndicatorDeviationLevels.Where(y => !y.Indicator.InAnalysis))
                .ThenInclude(x => x.Indicator)
                .In(specialistIds, x => x.Symptom.SpecialistId)
                .Where(x =>
                    x.AnalysisId == analysisId &&
                    x.Symptom.CategoryId == categoryId)
                .ToListAsync();
            var matchedIndicators = await _dataContext.Set<AnalysisSymptomIndicator>().AsNoTracking()
                .Where(x => x.AnalysisId == analysisId)
                .ToListAsync();

            var filteredAnalysisSymptomsWithIndicators = filteredAnalysisSymptoms.GroupJoin(matchedIndicators,
                (ansy) => new {ansy.AnalysisId, ansy.SymptomId},
                (ansyin) => new {ansyin.AnalysisId, ansyin.SymptomId}, (analysisSymptom, indicators) => new
                {
                    AnalysisSymptom = analysisSymptom,
                    MatchedIndicatorIds = indicators.Select(x => x.IndicatorId).ToHashSet()
                }).ToList();

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

            var result = matchedSymptomsTree.VisitAndConvert(x => matchedAnalysisSymptoms[x.Id])
                .VisitAndSort(x => x.CombinedSubtreeSeverity ?? 0, false);

            return result;
        }
        
        public async Task Update(Analysis analysis)
        {
            _dataContext.Set<Analysis>().Update(analysis);
            await _dataContext.SaveChangesAsync();
        }

        private static Dictionary<string, double> IndicatorWeightsDict => IndicatorWeightsDictField ??= IndicatorWeightsDictInitField;

        private static Dictionary<int, double> AbsoluteDeviationLevelMeasure => AbsoluteDeviationLevelMeasureField ??= AbsoluteDeviationLevelMeasureInitField;

        private static Dictionary<int, double> BaseSeverityForCountOfIndicators => BaseSeverityForCountOfIndicatorsField ??= BaseSeverityForCountOfIndicatorsInitField;

        public void RefreshAnalysisCalculationAlgorithmCache()
        {
            IndicatorWeightsDictField = null;
            AbsoluteDeviationLevelMeasureField = null;
            BaseSeverityForCountOfIndicatorsField = null;
        }
        
        private static AnalysisSymptom CalculateSeverity2(Symptom symptom,
            IDictionary<int, int> analysisIndicatorDict, int analysisId,
            IReadOnlyDictionary<int, List<SymptomIndicatorDeviationLevel>> matchedIndicators)
        {
            var symptomIndicators = symptom.SymptomIndicatorDeviationLevels
                .Select(x => new {x.Indicator, x.DeviationLevelId})
                .ToList();

            double? severity;

            if (symptomIndicators.Any())
            {
                try
                {
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
                                                IndicatorWeightsDict.GetValueOrDefault(
                                                    symptomIndicator.Indicator.ShortName, 1));
                        vectorAnalysis[i] =
                            AbsoluteDeviationLevelMeasure[
                                    Math.Abs(analysisIndicatorDict[symptomIndicator.Indicator.Id])]
                                .MultiplySign(analysisIndicatorDict[symptomIndicator.Indicator.Id])
                            * Math.Sqrt(
                                IndicatorWeightsDict.GetValueOrDefault(symptomIndicator.Indicator.ShortName, 1));
                    }
                    
                    var vectorAnalysisProjected = vectorRequired.Project(vectorAnalysis);
                    var vectorResult = vectorAnalysisProjected.Subtract(vectorRequired);

                    var result = new double[vectorResult.Length];
                    
                    for (var i = 0; i < vectorResult.Length; i++)
                    {
                        result[i] = Math.Pow((double) 2 / 3, vectorResult[i]);
                    }

                    severity = 0;
                }
                catch(OverflowException e)
                {
                    severity = null;
                    //TODO: log as error!
                }
            }
            else
            {
                severity = null;
            }
            
            return new AnalysisSymptom 
            {
                Symptom = symptom,
                SymptomId = symptom.Id, 
                Severity = (decimal?) severity?.RoundTo(3), 
                AnalysisId = analysisId,
                MatchedIndicatorIds = matchedIndicators[symptom.Id]
                    .Select(x => x.IndicatorId)
                    .ToHashSet()
            };
        }

        private static AnalysisSymptom CalculateSeverity(Symptom symptom,
            IDictionary<int, int> analysisIndicatorDict, int analysisId,
            IReadOnlyDictionary<int, List<SymptomIndicatorDeviationLevel>> matchedIndicators)
        {
            var symptomIndicators = symptom.SymptomIndicatorDeviationLevels
                .Select(x => new {x.Indicator, x.DeviationLevelId})
                .ToList();

            double? severity;

            if (symptomIndicators.Any())
            {
                try
                {
                    var vectorMaxSeverity = new double[symptomIndicators.Count];
                    var vectorMinSeverity = new double[symptomIndicators.Count];
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
                                                IndicatorWeightsDict.GetValueOrDefault(
                                                    symptomIndicator.Indicator.ShortName, 1));
                        vectorAnalysis[i] =
                            AbsoluteDeviationLevelMeasure[
                                    Math.Abs(analysisIndicatorDict[symptomIndicator.Indicator.Id])]
                                .MultiplySign(analysisIndicatorDict[symptomIndicator.Indicator.Id])
                            * Math.Sqrt(
                                IndicatorWeightsDict.GetValueOrDefault(symptomIndicator.Indicator.ShortName, 1));
                        vectorMaxSeverity[i] = AbsoluteDeviationLevelMeasure[5]
                                                   .MultiplySign(symptomIndicator.DeviationLevelId)
                                               * Math.Sqrt(
                                                   IndicatorWeightsDict.GetValueOrDefault(
                                                       symptomIndicator.Indicator.ShortName, 1));
                        vectorMinSeverity[i] = AbsoluteDeviationLevelMeasure[5]
                                                   .MultiplySign(symptomIndicator.DeviationLevelId * -1)
                                               * Math.Sqrt(
                                                   IndicatorWeightsDict.GetValueOrDefault(
                                                       symptomIndicator.Indicator.ShortName, 1));
                    }

                    var vectorMaxSeverityProjected = vectorRequired.Project(vectorMaxSeverity);
                    var vectorMinSeverityProjected = vectorRequired.Project(vectorMinSeverity);
                    var vectorAnalysisProjected = vectorRequired.Project(vectorAnalysis);

                    var baseSeverity =
                        BaseSeverityForCountOfIndicators.GetValueOrDefault(symptomIndicators.Count, 0.8);

                    if (vectorAnalysisProjected.Subtract(vectorMaxSeverityProjected).Distance() <
                        vectorAnalysisProjected.Subtract(vectorMinSeverityProjected).Distance())
                    {
                        var baseDistance = vectorMaxSeverityProjected.Distance() - vectorRequired.Distance();
                        var resultDistance =
                            vectorMaxSeverityProjected.Distance() - vectorAnalysisProjected.Distance();

                        if (baseDistance == 0)
                        {
                            if (resultDistance == 0)
                            {
                                severity = 1;
                            }
                            else
                            {
                                severity = baseSeverity;
                            }
                        }
                        else
                        {
                            severity = baseSeverity +
                                       (1 - baseSeverity) * (baseDistance - resultDistance) / baseDistance;
                        }
                    }
                    else
                    {
                        var baseDistance = vectorMinSeverityProjected.Distance() - vectorRequired.Distance();
                        var resultDistance =
                            vectorMinSeverityProjected.Distance() - vectorAnalysisProjected.Distance();

                        if (baseDistance == 0)
                        {
                            if (resultDistance == 0)
                            {
                                severity = baseSeverity;
                            }
                            else
                            {
                                severity = 0;
                            }
                        }
                        else
                        {
                            severity = baseSeverity * (baseDistance - resultDistance) / baseDistance;
                        }
                    }
                }
                catch(OverflowException e)
                {
                    severity = null;
                    //TODO: log as error!
                }
            }
            else
            {
                severity = null;
            }
            
            return new AnalysisSymptom 
            {
                Symptom = symptom,
                SymptomId = symptom.Id, 
                Severity = (decimal?) severity?.RoundTo(3), 
                AnalysisId = analysisId,
                MatchedIndicatorIds = matchedIndicators[symptom.Id]
                    .Select(x => x.IndicatorId)
                    .ToHashSet()
            };
        }

        private static readonly List<decimal?> EmptySeverityList = new(0);
        
        private static readonly Dictionary<string, double> IndicatorWeightsDictInitField = new()
        {
            {"nRBC", 3}, {"WBC", 3}, {"PLT", 3}, {"NLR", 3},

            {"NRBC", 2}, {"RET", 2}, {"HB", 2}, {"HCT", 2}, {"MCV", 2}, {"RDW", 2}, {"ESR", 2}, {"N", 2},
            {"E", 2},
            {"B", 2}, {"M", 2}, {"L", 2}, {"MPV", 2}, {"PLR", 2}, {"SII", 2}
        };

        private static readonly Dictionary<int, double> AbsoluteDeviationLevelMeasureInitField = new()
        {
            {0, 0}, {1, 1}, {2, 2}, {3, Math.Sqrt(13)}, {4, Math.Sqrt(27)}, {5, Math.Sqrt(40)}
        };

        private static readonly Dictionary<int, double> BaseSeverityForCountOfIndicatorsInitField = new()
        {
            {1, 0.4}, {2, 0.5}, {3, 0.6}, {4, 0.6}, {5, 0.6}, {6, 0.6}, {7, 0.7}, {8, 0.7}, {9, 0.8}
        };

        private static Dictionary<string, double> IndicatorWeightsDictField;
        private static Dictionary<int, double> AbsoluteDeviationLevelMeasureField;
        private static Dictionary<int, double> BaseSeverityForCountOfIndicatorsField;

        private static AnalysisSymptom CalculateCombinedSeverity(AnalysisSymptom analysisSymptom,
            IEnumerable<AnalysisSymptom> children)
        {
            var currentSeverity = analysisSymptom.Severity;
            var childrenSeverities = children?
                .Select(x => x.CombinedSubtreeSeverity ?? x.Severity)
                .Where(x => x.HasValue)
                .ToList() ?? EmptySeverityList;

            const decimal higherLevelCoefficient = 2.5m;
            analysisSymptom.CombinedSubtreeSeverity = currentSeverity.HasValue
                ? (higherLevelCoefficient * currentSeverity.Value + childrenSeverities.Sum()) /
                  (higherLevelCoefficient + childrenSeverities.Count)
                : childrenSeverities.Count > 0
                    ? childrenSeverities.Sum() / childrenSeverities.Count
                    : null;

            return analysisSymptom;
        }
    }
}