using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FluentValidation;
using MedExpert.Core;
using MedExpert.Core.Helpers;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Services.Implementation.ComputedIndicators;
using MedExpert.Services.Interfaces;
using MedExpert.Web.ViewModels;
using MedExpert.Web.ViewModels.Analysis;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class AnalysisController:ControllerBase
    {
        private static readonly List<IComputedIndicator> ComputedIndicators = new() { new ELR(), new EMR(), new LMR(), new MLR(), new NLR(), new PLR(), new PNR(), new BLR(), new SII()};
        private const string CommonTreatmentSpecialistLookupName = "CommonTreatmentSpecialist";
        private const string CommonAnalysisSpecialistLookupName = "CommonAnalysisSpecialist";
        
        private readonly IIndicatorService _indicatorService;
        private readonly IReferenceIntervalService _referenceIntervalService;
        private readonly ISpecialistService _specialistService;
        private readonly IDeviationLevelService _deviationLevelService;
        private readonly IAnalysisIndicatorService _analysisIndicatorService;
        private readonly IAnalysisService _analysisService;
        private readonly IAnalysisSymptomService _analysisSymptomService;
        private readonly IAnalysisSymptomIndicatorService _analysisSymptomIndicatorService;
        private readonly ILookupService _lookupService;
        private readonly ISymptomCategoryService _symptomCategoryService;
        private readonly IValidator<AnalysisFormModel> _analysisFormValidator;
        
        public AnalysisController(IIndicatorService indicatorService, IReferenceIntervalService referenceIntervalService, ISpecialistService specialistService, IDeviationLevelService deviationLevelService, IAnalysisIndicatorService analysisIndicatorService, IAnalysisService analysisService, IValidator<AnalysisFormModel> analysisFormValidator, IAnalysisSymptomService analysisSymptomService, IAnalysisSymptomIndicatorService analysisSymptomIndicatorService, ILookupService lookupService, ISymptomCategoryService symptomCategoryService)
        {
            _indicatorService = indicatorService;
            _referenceIntervalService = referenceIntervalService;
            _specialistService = specialistService;
            _deviationLevelService = deviationLevelService;
            _analysisIndicatorService = analysisIndicatorService;
            _analysisService = analysisService;
            _analysisFormValidator = analysisFormValidator;
            _analysisSymptomService = analysisSymptomService;
            _analysisSymptomIndicatorService = analysisSymptomIndicatorService;
            _lookupService = lookupService;
            _symptomCategoryService = symptomCategoryService;
        }

        #region Indicators

        [HttpPost]
        [ApiRoute("Analysis/Indicators")]
        public async Task<List<IndicatorValueDependencyModel>> Indicators([FromBody] ProfileModel model)
        {
            var indicators = await _indicatorService.GetAnalysisIndicators();
            var referenceIntervals =
                await _referenceIntervalService.GetReferenceIntervalsByCriteria(model.Sex, model.Age);
            
            var indicatorNamesDict = indicators
                .ToDictionary(x => x.ShortName, x => x.Id);
            var referenceIntervalsDict = referenceIntervals
                .ToDictionary(x => x.IndicatorId, x => x);
            
            var result = indicators.Select(x =>
            {
                var referenceExists = referenceIntervalsDict.TryGetValue(x.Id, out var referenceInterval);
                return new IndicatorValueDependencyModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ShortName = x.ShortName,
                    Value = null,
                    ReferenceIntervalMin = referenceExists ? referenceInterval.ValueMin : null,
                    ReferenceIntervalMax = referenceExists ? referenceInterval.ValueMax : null,
                    DependencyIndicatorIds = ComputedIndicators
                        .FirstOrDefault(y => y.ShortName == x.ShortName)?.DependentShortNames
                        .Select(y => indicatorNamesDict.ContainsKey(y) ? indicatorNamesDict[y] : (int?)null)
                        .Where(y => y.HasValue)
                        .Select(y => y.Value)
                        .ToList()
                };
            }).ToList();

            return result;

            return GetStubIndicators();
        }
        
        #endregion

        #region Public Specialists

        [HttpPost]
        [ApiRoute("Analysis/Specialists")]
        public async Task<List<LookupModel>> Specialists([FromBody] ProfileModel model)
        {
            var specialists = await _specialistService.GetSpecialistsByCriteria(model.Sex);
            
            var lookup1 = await _lookupService.GetByName(CommonTreatmentSpecialistLookupName);
            var lookup2 = await _lookupService.GetByName(CommonAnalysisSpecialistLookupName);
            var specialist1 = await _specialistService.GetSpecialistById(int.Parse(lookup1.Value));
            var specialist2 = await _specialistService.GetSpecialistById(int.Parse(lookup2.Value));

            // ReSharper disable once ConvertToLocalFunction
            Func<Specialist, bool> excludeCommonSpecialists =
                x => x.Id != specialist1?.Id && x.Id != specialist2?.Id;
            
            var result = specialists.Where(excludeCommonSpecialists).Select(x => new LookupModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return result;

            return GetStubSpecialists(model);
        }
        
        #endregion

        #region ComputeIndicators
        
        [HttpPost]
        [ApiRoute("Analysis/ComputeIndicators")]
        public async Task<Dictionary<int, decimal>> ComputeIndicators([FromBody] List<IdValueModel> idValues)
        {
            var indicators = await _indicatorService.GetAnalysisIndicators();
            var idValuesDict = idValues
                .ToDictionary(x => x.Id, x => x);
            
            var nameValuesDict = new Dictionary<string, decimal>();
            foreach (var indicator in indicators)
            {
                if (idValuesDict.TryGetValue(indicator.Id, out var idValue) && idValue.Value.HasValue)
                {
                    nameValuesDict[indicator.ShortName] = idValue.Value.Value;
                }
            }

            var nameValuesComputedDict = new Dictionary<string, decimal>();
            foreach (var computedIndicator in ComputedIndicators
                .Where(computedIndicator => computedIndicator.DependentShortNames
                    .All(n => nameValuesDict.ContainsKey(n))))
            {
                nameValuesComputedDict[computedIndicator.ShortName] = computedIndicator.Compute(nameValuesDict);
            }

            var result = new Dictionary<int, decimal>();

            foreach (var indicator in indicators)
            {
                if (nameValuesComputedDict.TryGetValue(indicator.ShortName, out var value))
                {
                    result[indicator.Id] = value;
                }
            }

            return result;
        }
        
        #endregion

        #region Calculate

        [HttpPost]
        [ApiRoute("Analysis/Calculate")]
        public async Task<int> Calculate([FromBody] AnalysisFormModel formModel)
        {
            var validationResult = await _analysisFormValidator.ValidateAsync(formModel);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            
            var lookup1 = await _lookupService.GetByName(CommonTreatmentSpecialistLookupName);
            var lookup2 = await _lookupService.GetByName(CommonAnalysisSpecialistLookupName);
            var specialist1 = await _specialistService.GetSpecialistById(int.Parse(lookup1.Value));
            var specialist2 = await _specialistService.GetSpecialistById(int.Parse(lookup2.Value));
            
            var now = DateTime.Now;
            var analysis = new Analysis
            {
                Age = formModel.Profile.Age,
                Sex = formModel.Profile.Sex,
                UserId = 1,
                CalculationTime = now,
                Date = now.Date
            };

            var deviationLevels = await _deviationLevelService.GetAll();
            var toInsertDeviationLevels = deviationLevels.Select(x => new AnalysisDeviationLevel
            {
                MinPercentFromCenter = x.MinPercentFromCenter,
                MaxPercentFromCenter = x.MaxPercentFromCenter,
                DeviationLevel = x,
                Analysis = analysis
            }).ToList();

            var toInsertIndicators = formModel.Indicators
                .Where(x => x.Value != null)
                .Select(x => new AnalysisIndicator
                {
                    Analysis = analysis,
                    IndicatorId = x.Id,
                    Value = x.Value.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    ReferenceIntervalValueMin = x.ReferenceIntervalMin.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    ReferenceIntervalValueMax = x.ReferenceIntervalMax.Value,
                    DeviationLevel = _deviationLevelService.Calculate(x.ReferenceIntervalMin.Value,
                        x.ReferenceIntervalMax.Value, x.Value.Value, toInsertDeviationLevels)
                }).ToList();

            try
            {
                using (var transaction1 = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions{ IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _analysisService.Insert(analysis);
                    await _analysisIndicatorService.InsertBulk(toInsertIndicators);
                    await _deviationLevelService.InsertBulk(toInsertDeviationLevels);

                    transaction1.Complete();
                }

                formModel.SpecialistIds.AddRange(new[]
                {
                    specialist1?.Id ?? default,
                    specialist2?.Id ?? default
                });
                
                var symptomsTree = await _analysisService.CalculateNewAnalysis(analysis.Id, formModel.SpecialistIds);
                var symptomsList = symptomsTree.MakeFlat();

                var toInsertAnalysisSymptoms = symptomsList.Select(x =>
                    new AnalysisSymptom
                    {
                        AnalysisId = analysis.Id,
                        SymptomId = x.SymptomId,
                        Severity = x.Severity,
                        CombinedSubtreeSeverity = x.CombinedSubtreeSeverity
                    }).ToList();
                var toInsertMatchedIndicators = symptomsList.SelectMany(x => x.MatchedIndicatorIds.Select(y =>
                    new AnalysisSymptomIndicator
                    {
                        AnalysisId = analysis.Id,
                        SymptomId = x.SymptomId,
                        IndicatorId = y
                    })).ToList();

                analysis.Calculated = true;

                using (var transaction2 = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions{ IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _analysisService.Update(analysis);
                    await _analysisSymptomService.InsertBulk(toInsertAnalysisSymptoms);
                    await _analysisSymptomIndicatorService.InsertBulk(toInsertMatchedIndicators);

                    transaction2.Complete();
                }

                return analysis.Id;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        #endregion

        #region FilterResults
        
        [HttpPost]
        [ApiRoute("Analysis/FilterResults")]
        public async Task<AnalysisResultModel> FilterResults([FromBody] AnalysisResultFilterModel model)
        {
            try
            {
                string categoryName;
                string specialistLookupName = null;
                switch (model.Filter)
                {
                    case MedicalStateFilter.Diseases:
                        categoryName = "Illness";
                        break;
                    case MedicalStateFilter.CommonAnalysis:
                        categoryName = "Analysis";
                        specialistLookupName = CommonAnalysisSpecialistLookupName;
                        break;
                    case MedicalStateFilter.SpecialAnalysis:
                        categoryName = "Analysis";
                        break;
                    case MedicalStateFilter.CommonTreatment:
                        categoryName = "Treatment";
                        specialistLookupName = CommonTreatmentSpecialistLookupName;
                        break;
                    case MedicalStateFilter.SpecialTreatment:
                        categoryName = "Treatment";
                        break;
                    default:
                        throw new ValidationException("Некорректно заданный фильтр.");
                }

                if (specialistLookupName != null)
                {
                    var lookup = await _lookupService.GetByName(specialistLookupName);
                    var specialist = await _specialistService.GetSpecialistById(int.Parse(lookup.Value));

                    model.SpecialistIds = new List<int> {specialist.Id};
                }

                var category = await _symptomCategoryService.GetByName(categoryName);

                var symptomsTree =
                    await _analysisService.FetchCalculatedAnalysis(model.AnalysisId, model.SpecialistIds, category.Id);

                var commentsList = new List<CommentModel>();
                var startOrder = 0;
                var ordersForComments = new Dictionary<int, int>();
                var toReturn = new AnalysisResultModel
                {
                    AnalysisId = model.AnalysisId,
                    FoundMedicalStates = symptomsTree
                        .VisitAndConvert(x => ConvertAnalysisSymptomToModel(x, commentsList))
                        .VisitAndConvert(x => SetOrderInResultTree(x, ref startOrder, ordersForComments)),
                    Comments = commentsList
                        .OrderBy(x => ordersForComments[x.SymptomId])
                        .ThenBy(x => x.Type)
                        .ThenBy(x => x.Name)
                        .ToList()
                };

                return toReturn;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        private static MedicalStateModel ConvertAnalysisSymptomToModel(AnalysisSymptom analysisSymptom, List<CommentModel> commentsToFill)
        {
            var matchedIndicators = analysisSymptom.Symptom.SymptomIndicatorDeviationLevels
                .Where(x => analysisSymptom.MatchedIndicatorIds.Contains(x.IndicatorId))
                .ToList();
            var recommendedIndicators = analysisSymptom.Symptom.SymptomIndicatorDeviationLevels
                .Where(x => !x.Indicator.InAnalysis)
                .ToList();

            var symptomId = analysisSymptom.SymptomId;
            var symptomName = analysisSymptom.Symptom.Name;
            var specialistId = analysisSymptom.Symptom.SpecialistId;

            if (!string.IsNullOrEmpty(analysisSymptom.Symptom.Comment))
            {
                commentsToFill.Add(new CommentModel
                {
                    SpecialistId = specialistId,
                    SymptomId = symptomId,
                    SymptomName = symptomName,
                    Name = null,
                    Text = analysisSymptom.Symptom.Comment,
                    Type = CommentType.Symptom
                });
            }
            
            commentsToFill.AddRange(matchedIndicators.Where(x => string.IsNullOrEmpty(x.Comment)).Select(x => new CommentModel
            {
                SpecialistId = specialistId,
                SymptomId = symptomId,
                SymptomName = symptomName,
                Name = x.Indicator.Name,
                Text = x.Comment,
                Type = CommentType.MatchedIndicator
            }));
            
            commentsToFill.AddRange(recommendedIndicators.Where(x => !string.IsNullOrEmpty(x.Comment))
                .Select(x => new CommentModel
            {
                SpecialistId = specialistId,
                SymptomId = symptomId,
                SymptomName = symptomName,
                Name = x.Indicator.Name,
                Text = x.Comment,
                Type = CommentType.RecommendedForAnalysisIndicator
            }));
            
            return new MedicalStateModel
            {
                SpecialistId = specialistId,
                SymptomId = symptomId,
                Name = symptomName,
                Severity = analysisSymptom.Severity,
                CombinedSubtreeSeverity = analysisSymptom.CombinedSubtreeSeverity,
                RecommendedAnalyses = 
                    recommendedIndicators.Select(x => new IndicatorModel
                    {
                        Id = x.IndicatorId,
                        Name = x.Indicator.Name,
                        ShortName = x.Indicator.ShortName
                    }).ToList()
            };
        }

        private static MedicalStateModel SetOrderInResultTree(MedicalStateModel model, ref int order, IDictionary<int, int> symptomIdsToOrders)
        {
            symptomIdsToOrders[model.SymptomId] = order++;
            
            return model;
        }
        
        #endregion

        [HttpPost]
        [ApiRoute("Analysis/CalculateStub")]
        public async Task<AnalysisResultModel> CalculateStub([FromBody] AnalysisFormModel formModel)
        {
            return await Task.FromResult(GetStubAnalysisResult());
        }

        #region Stubs

        private static List<IndicatorValueDependencyModel> GetStubIndicators()
        {
            return new()
            {
                new IndicatorValueDependencyModel
                {
                    Id = 1, ShortName = "Hb", Name = "Гемоглобин"
                },
                new IndicatorValueDependencyModel
                {
                    Id = 2, ShortName = "RBC", Name = "Эритроциты", ReferenceIntervalMin = 1,
                    ReferenceIntervalMax = 1.5m
                },
                new IndicatorValueDependencyModel
                {
                    Id = 3, ShortName = "HCT", Name = "Гематокрит", ReferenceIntervalMin = 2,
                    ReferenceIntervalMax = 2.8m
                }
            };
        }
        
        private static List<LookupModel> GetStubSpecialists(ProfileModel model)
        {
            var result = new List<LookupModel>
            {
                new() {Id = 1, Name = "Гематолог"},
                new() {Id = 2, Name = "Онколог"},
                new() {Id = 3, Name = "Психиатр"}
            };

            switch (model.Sex)
            {
                case Sex.Female:
                    result.Add(new LookupModel{ Id = 4, Name = "Гинеколог"});
                    break;
                case Sex.Male:
                    result.Add(new LookupModel{ Id = 5, Name = "Андролог"});
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.Sex));
            }

            return result;
        }

        private static AnalysisResultModel GetStubAnalysisResult()
        {
            return new()
            {
                AnalysisId = 1,
                FoundMedicalStates = new List<TreeItem<MedicalStateModel>>
                {
                    new()
                    {
                        Item = new()
                        {
                            SymptomId = 1, 
                            Name = "Воспалительные заболевания носа, горла и уха", 
                            Severity = 0.8m, 
                            RecommendedAnalyses = new List<IndicatorModel>
                            {
                                new() {Id = 1, ShortName = "CHOLES", Name = "CHOLES"},
                                new() {Id = 2, ShortName = "LDL", Name = "LDL"},
                                new() {Id = 3, ShortName = "TG", Name = "TG"},
                            }
                        },
                        Children = new List<TreeItem<MedicalStateModel>>
                        {
                            new()
                            {
                                Item = new()
                                {
                                    SymptomId = 2, 
                                    Name = "Ринит", 
                                    Severity = 0.76m, 
                                    RecommendedAnalyses = new List<IndicatorModel>
                                    {
                                        new() {Id = 4, ShortName = "Hb", Name = "Гемоглобин"},
                                        new() {Id = 5, ShortName = "QWE", Name = "QWE"},
                                        new() {Id = 3, ShortName = "TG", Name = "TG"},
                                    }
                                },
                                Children = new List<TreeItem<MedicalStateModel>>
                                {
                                    new()
                                    {
                                        Item = new()
                                        {
                                            SymptomId = 3, 
                                            Name = "Ринит не аллергический", 
                                            Severity = 0.88m, RecommendedAnalyses =
                                            new List<IndicatorModel>
                                            {
                                                new() {Id = 6, ShortName = "RBC", Name = "RBC"},
                                                new() {Id = 7, ShortName = "TB", Name = "Тромбоциты"},
                                                new() {Id = 2, ShortName = "LDL", Name = "LDL"},
                                            }
                                        }
                                    },
                                    new()
                                    {
                                        Item = new()
                                        {
                                            SymptomId = 4, 
                                            Name = "Хронический ринит", 
                                            Severity = 0.79m
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        Item = new()
                        {
                            SymptomId = 5, Name = "Аллергические заболевания", Severity = 0.56m, RecommendedAnalyses =
                            new List<IndicatorModel>
                            {
                                new() {Id = 8, ShortName = "AAA", Name = "AAA"},
                                new() {Id = 9, ShortName = "BBB", Name = "BBB"},
                                new() {Id = 10, ShortName = "CCC", Name = "CCC"},
                            },
                        },
                        Children = new List<TreeItem<MedicalStateModel>>
                        {
                            new()
                            {
                                Item = new()
                                {
                                    SymptomId = 6, Name = "Респираторные аллергические заболевания", Severity = 0.6m
                                },
                                Children = new List<TreeItem<MedicalStateModel>>
                                {
                                    new()
                                    {
                                        Item = new()
                                        {
                                            SymptomId = 7, Name = "Астма", Severity = 0.7m
                                        },
                                        Children = new List<TreeItem<MedicalStateModel>>
                                        {
                                            new()
                                            {
                                                Item = new()
                                                {
                                                    SymptomId = 8, Name = "Варианты астмы", Severity = 0.4m
                                                },
                                                Children = new List<TreeItem<MedicalStateModel>>
                                                {
                                                    new()
                                                    {
                                                        Item = new()
                                                        {
                                                            SymptomId = 9, Name = "Аллергическая астма", Severity = 0.48m
                                                        },
                                                        Children = new List<TreeItem<MedicalStateModel>>
                                                        {
                                                            new()
                                                            {
                                                                Item = new()
                                                                {
                                                                    SymptomId = 10, Name = "Обострение алергической астмы", Severity = 0.95m
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new()
                            {
                                Item = new()
                                {
                                    SymptomId = 3, Name = "Анафилаксия", Severity = 0.55m
                                }
                            }
                        }
                    }
                },
                Comments = new List<CommentModel>
                {
                    new (){ SpecialistId = 1, SymptomId = 1, Type = CommentType.Symptom, Name = "Воспалительные заболевания носа, горла и уха", Text = "LTE 4 в моче является подтвержденным маркером активности цистеиниллейкотриенов, и его следует рассматривать для включения в клинические испытания молекул, которые могут прямо или косвенно влиять на этот путь." },
                    new (){ SpecialistId = 1, SymptomId = 2, Type = CommentType.Symptom, Name = "Аллергические заболевания", Text = "Комментарий 1" },
                    new (){ SpecialistId = 1, SymptomId = 3, Type = CommentType.Symptom, Name = "Анафилаксия", Text = "Комментарий 2" },
                    new (){ SpecialistId = 2, SymptomId = 4, Type = CommentType.MatchedIndicator, Name = "Гемоглобин (Hb)", Text = "Провоспалительные цитокины, включая фактор некроза опухоли и интерлейкин 6, не только повышаются при сердечной недостаточности, но и обратно пропорциональны гемоглобину." },
                    new (){ SpecialistId = 2, SymptomId = 5, Type = CommentType.RecommendedForAnalysisIndicator, Name = "CHOLES", Text = "Хронический гайморит часто встречается у детей с респираторной аллергией и связан с повышенной заболеваемостью. Бактериология хронической болезни носовых пазух у этих детей не была адекватно оценена. В период с мая 1987 г. по январь 1988 г. было полностью обследовано 12 детей (в возрасте от 3 до 9 лет) с подтвержденной респираторной аллергией и хроническими респираторными симптомами, характерными для хронического синусита (>30 дней). Был проведен анамнез, медицинский осмотр, общий анализ крови, мазок из носа и рентгенологическое исследование Уотерса. У всех пациентов было затемнение одной или обеих верхнечелюстных пазух, не реагировали на многократные курсы антибиотиков, и впоследствии им была проведена аспирация и промывание верхнечелюстных пазух. Образцы культивировали на наличие аэробных и анаэробных организмов по стандартной методике и получали чувствительность.Moraxella [Branhamella] catarrhalis ) у пяти пациентов, у одного пациента были обнаружены M. catarrhalis плюс виды Streptococcus , у трех были отрицательные результаты, и у трех пациентов развились несколько микроорганизмов (у двух с несколькими видами аэробных стрептококков и у одного пациента с аэробными стрептококками и Peptostreptococcus ). Все дети получали адекватную культуральную антимикробную терапию. Последовательное наблюдение раз в две недели выявило прогрессирующее рентгенологическое прояснение и значительное симптоматическое улучшение. M. catarrhalis является распространенным патогеном, тогда как анаэробные микроорганизмы редко вызывают хронический гайморит у детей-аллергиков. Некоторым детям, несмотря на отрицательные результаты посева, может помочь промывание верхнечелюстной пазухи." },
                    new (){ SpecialistId = 3, SymptomId = 6, Type = CommentType.RecommendedForAnalysisIndicator, Name = "BBB", Text = "Комментарий 3" }
                }
            };
        }
        
        #endregion
    }
}