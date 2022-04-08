using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private static readonly List<IComputedIndicator> ComputedIndicators = new() { new ELR(), new EMR(), new LMR(), new MLR(), new NLR(), new PLR(), new SII()};
        
        private readonly IIndicatorService _indicatorService;
        private readonly IReferenceIntervalService _referenceIntervalService;
        private readonly ISpecialistService _specialistService;

        public AnalysisController(IIndicatorService indicatorService, IReferenceIntervalService referenceIntervalService, ISpecialistService specialistService)
        {
            _indicatorService = indicatorService;
            _referenceIntervalService = referenceIntervalService;
            _specialistService = specialistService;
        }

        [HttpGet]
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

            //return GetStubIndicators();
        }
        
        [HttpPost]
        [ApiRoute("Analysis/Specialists")]
        public async Task<List<LookupModel>> Specialists([FromBody] ProfileModel model)
        {
            var specialists = await _specialistService.GetSpecialistsByCriteria(model.Sex);

            var result = specialists.Select(x => new LookupModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return result;

            //return GetStubSpecialists(model);
        }

        [HttpPost]
        [ApiRoute("Analysis/ComputeIndicators")]
        public async Task<Dictionary<int, decimal>> ComputeIndicators(List<IdValueModel> idValues)
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

        [HttpPost]
        [ApiRoute("Analysis/Calculate")]
        public async Task<AnalysisResultModel> Calculate([FromBody] AnalysisFormModel formModel)
        {
            return await Task.FromResult(new AnalysisResultModel
            {
                AnalysisId = 1,
                FoundMedicalStates = new List<MedicalStateModel>
                {
                    new()
                    {
                        SymptomId = 1, Name = "Воспалительные заболевания носа, горла и уха", Expressiveness = 0.8m, Match = 1, RecommendedAnalyses =
                            new List<IndicatorModel>
                            {
                                new() {Id = 1, ShortName = "CHOLES", Name = "CHOLES"},
                                new() {Id = 2, ShortName = "LDL", Name = "LDL"},
                                new() {Id = 3, ShortName = "TG", Name = "TG"},
                            },
                        ChildSymptoms = new List<MedicalStateModel>
                        {
                            new()
                            {
                                SymptomId = 2, Name = "Ринит", Expressiveness = 0.76m, Match = 0.8m, RecommendedAnalyses =
                                    new List<IndicatorModel>
                                    {
                                        new() {Id = 4, ShortName = "Hb", Name = "Гемоглобин"},
                                        new() {Id = 5, ShortName = "QWE", Name = "QWE"},
                                        new() {Id = 3, ShortName = "TG", Name = "TG"},
                                    },
                                ChildSymptoms = new List<MedicalStateModel>
                                {
                                    new()
                                    {
                                        SymptomId = 3, Name = "Ринит не аллергический", Expressiveness = 0.88m, Match = 0.7m, RecommendedAnalyses =
                                            new List<IndicatorModel>
                                            {
                                                new() {Id = 6, ShortName = "RBC", Name = "RBC"},
                                                new() {Id = 7, ShortName = "TB", Name = "Тромбоциты"},
                                                new() {Id = 2, ShortName = "LDL", Name = "LDL"},
                                            }
                                    },
                                    new()
                                    {
                                        SymptomId = 4, Name = "Хронический ринит", Expressiveness = 0.79m, Match = 0.75m
                                    }
                                }
                            }
                        }
                    },
                    new()
                    {
                        SymptomId = 5, Name = "Аллергические заболевания", Expressiveness = 0.56m, Match = 0.67m, RecommendedAnalyses =
                            new List<IndicatorModel>
                            {
                                new() {Id = 8, ShortName = "AAA", Name = "AAA"},
                                new() {Id = 9, ShortName = "BBB", Name = "BBB"},
                                new() {Id = 10, ShortName = "CCC", Name = "CCC"},
                            },
                        ChildSymptoms = new List<MedicalStateModel>
                        {
                            new()
                            {
                                SymptomId = 6, Name = "Респираторные аллергические заболевания", Expressiveness = 0.6m, Match = 1,
                                ChildSymptoms = new List<MedicalStateModel>
                                {
                                    new()
                                    {
                                        SymptomId = 7, Name = "Астма", Expressiveness = 0.7m, Match = 1,
                                        ChildSymptoms = new List<MedicalStateModel>
                                        {
                                            new()
                                            {
                                                SymptomId = 8, Name = "Варианты астмы", Expressiveness = 0.4m, Match = 1,
                                                ChildSymptoms = new List<MedicalStateModel>
                                                {
                                                    new()
                                                    {
                                                        SymptomId = 9, Name = "Аллергическая астма", Expressiveness = 0.48m, Match = 1,
                                                        ChildSymptoms = new List<MedicalStateModel>
                                                        {
                                                            new()
                                                            {
                                                                SymptomId = 10, Name = "Обострение алергической астмы", Expressiveness = 0.95m, Match = 1
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
                                SymptomId = 3, Name = "Анафилаксия", Expressiveness = 0.55m, Match = 1
                            },
                        }
                    }
                },
                Comments = new List<CommentModel>
                {
                    new (){ CommentId = 1, Type = CommentType.Illness, Name = "Воспалительные заболевания носа, горла и уха", Text = "LTE 4 в моче является подтвержденным маркером активности цистеиниллейкотриенов, и его следует рассматривать для включения в клинические испытания молекул, которые могут прямо или косвенно влиять на этот путь." },
                    new (){ CommentId = 5, Type = CommentType.Illness, Name = "Аллергические заболевания", Text = "Комментарий 1" },
                    new (){ CommentId = 3, Type = CommentType.Symptom, Name = "Анафилаксия", Text = "Комментарий 2" },
                    new (){ CommentId = 4, Type = CommentType.MatchedIndicator, Name = "Гемоглобин (Hb)", Text = "Провоспалительные цитокины, включая фактор некроза опухоли и интерлейкин 6, не только повышаются при сердечной недостаточности, но и обратно пропорциональны гемоглобину." },
                    new (){ CommentId = 1, Type = CommentType.RecommendedForAnalysisIndicator, Name = "CHOLES", Text = "Хронический гайморит часто встречается у детей с респираторной аллергией и связан с повышенной заболеваемостью. Бактериология хронической болезни носовых пазух у этих детей не была адекватно оценена. В период с мая 1987 г. по январь 1988 г. было полностью обследовано 12 детей (в возрасте от 3 до 9 лет) с подтвержденной респираторной аллергией и хроническими респираторными симптомами, характерными для хронического синусита (>30 дней). Был проведен анамнез, медицинский осмотр, общий анализ крови, мазок из носа и рентгенологическое исследование Уотерса. У всех пациентов было затемнение одной или обеих верхнечелюстных пазух, не реагировали на многократные курсы антибиотиков, и впоследствии им была проведена аспирация и промывание верхнечелюстных пазух. Образцы культивировали на наличие аэробных и анаэробных организмов по стандартной методике и получали чувствительность.Moraxella [Branhamella] catarrhalis ) у пяти пациентов, у одного пациента были обнаружены M. catarrhalis плюс виды Streptococcus , у трех были отрицательные результаты, и у трех пациентов развились несколько микроорганизмов (у двух с несколькими видами аэробных стрептококков и у одного пациента с аэробными стрептококками и Peptostreptococcus ). Все дети получали адекватную культуральную антимикробную терапию. Последовательное наблюдение раз в две недели выявило прогрессирующее рентгенологическое прояснение и значительное симптоматическое улучшение. M. catarrhalis является распространенным патогеном, тогда как анаэробные микроорганизмы редко вызывают хронический гайморит у детей-аллергиков. Некоторым детям, несмотря на отрицательные результаты посева, может помочь промывание верхнечелюстной пазухи." },
                    new (){ CommentId = 9, Type = CommentType.RecommendedForAnalysisIndicator, Name = "BBB", Text = "Комментарий 3" }
                }
            });
        }
        
        #region Stubs

        private static List<IndicatorValueDependencyModel> GetStubIndicators()
        {
            return new()
            {
                new()
                {
                    Id = 1, ShortName = "Hb", Name = "Гемоглобин"
                },
                new()
                {
                    Id = 2, ShortName = "RBC", Name = "Эритроциты", ReferenceIntervalMin = 1,
                    ReferenceIntervalMax = 1.5m
                },
                new()
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

        #endregion
    }
}