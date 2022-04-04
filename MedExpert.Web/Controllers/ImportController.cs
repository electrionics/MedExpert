using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FluentValidation;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Excel;
using MedExpert.Excel.Metadata.Analysis;
using MedExpert.Excel.Metadata.Common;
using MedExpert.Excel.Metadata.Indicators;
using MedExpert.Excel.Metadata.ReferenceIntervals;
using MedExpert.Excel.Metadata.Symptoms;
using MedExpert.Excel.Model.Analysis;
using MedExpert.Excel.Model.Indicators;
using MedExpert.Excel.Model.ReferenceIntervals;
using MedExpert.Excel.Model.Symptoms;
using Microsoft.AspNetCore.Mvc;
using MedExpert.Web.ViewModels;
using MedExpert.Services.Interfaces;
using MedExpert.Web.ViewModels.Import;

// ReSharper disable CommentTypo

// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class ImportController:ControllerBase
    {
        private readonly ISpecialistService _specialistService;
        private readonly IReferenceIntervalService _referenceIntervalService;
        private readonly IIndicatorService _indicatorService;
        private readonly IDeviationLevelService _deviationLevelService;
        private readonly ISymptomService _symptomService;
        private readonly IAnalysisService _analysisService;
        private readonly IAnalysisIndicatorService _analysisIndicatorService;
        private readonly ISymptomCategoryService _symptomCategoryService;
        private readonly ExcelParser _excelParser;
        private readonly IValidator<ImportSymptomForm> _importSymptomValidator;

        public ImportController(IReferenceIntervalService referenceIntervalService, ExcelParser excelParser, IIndicatorService indicatorService, ISpecialistService specialistService, IDeviationLevelService deviationLevelService, ISymptomService symptomService, IAnalysisService analysisService, IAnalysisIndicatorService analysisIndicatorService, IValidator<ImportSymptomForm> importSymptomValidator, ISymptomCategoryService symptomCategoryService)
        {
            _referenceIntervalService = referenceIntervalService;
            _excelParser = excelParser;
            _indicatorService = indicatorService;
            _specialistService = specialistService;
            _deviationLevelService = deviationLevelService;
            _symptomService = symptomService;
            _analysisService = analysisService;
            _analysisIndicatorService = analysisIndicatorService;
            _importSymptomValidator = importSymptomValidator;
            _symptomCategoryService = symptomCategoryService;
        }
        
        
        #region Indicators

        [HttpPost]
        [ApiRoute("Import/Indicators")]
        public async Task<ImportReport> ImportIndicators()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                var report = new ImportReport();
                const int headerRow = 1;
                var metadataObject = new ImportIndicatorMetadata();
                
                try
                {
                    await using var stream = file.OpenReadStream();
                    var parseResult = _excelParser.Parse(stream);
                    var entityRows = parseResult
                        .Where(x => x.Key > headerRow)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var header = CreateAndValidateHeaderAndInitializeReport(metadataObject, parseResult, headerRow, entityRows.Count, report);

                    if (report.HeaderValid)
                    {
                        FillEmptyCells(header, entityRows);
                        ValidateCellsAndSetReportErrors(metadataObject, header, entityRows, report);

                        var importCandidates = CreateImportCandidates(metadataObject, header, entityRows, report);
                        ProcessIndicators(importCandidates);

                        ValidateEntitiesAndAddErrorsToReport(metadataObject, header, importCandidates, report);
                        
                        if (!report.ErrorsByRows.Any())
                        {
                            var keys = importCandidates.Values.Select(x => x.ShortName).ToList();

                            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                            
                            var toUpdates = await _indicatorService.GetIndicators(keys);

                            var toUpdateKeys = toUpdates.Select(x => x.ShortName).ToList();
                            var toInserts = importCandidates.Values
                                .Where(x => !toUpdateKeys.Contains(x.ShortName))
                                .Select(x => x.CreateEntity())
                                .ToList();

                            foreach (var toUpdate in toUpdates)
                            {
                                var toImport = importCandidates.Values.First(x => x.ShortName == toUpdate.ShortName);

                                toImport.UpdateEntity(toUpdate);
                            }

                            await SingleBulkUpdateAndSetReport(_indicatorService, toUpdates, report);
                            await SingleBulkInsertAndSetReport(_indicatorService, toInserts, report);
                            
                            if (report.TotalInsertedErrorsCount == 0 && report.TotalUpdatedErrorsCount == 0)
                            {
                                transaction.Complete();
                            }
                            else
                            {
                                transaction.Dispose();
                            }
                        }
                    }

                    stopwatch.Stop();

                    report.TotalExecutionTimeSeconds = stopwatch.ElapsedMilliseconds / 1000m;

                    report.CalculateReport();
                    report.BuildErrorsByColumns();
                }
                catch (Exception e)
                {
                    report.Error = e.Message;
                }
                
                return await Task.FromResult(report);
            }
            
            return null;
        }

        private static void ProcessIndicators(Dictionary<int, ImportIndicatorModel> imports)
        {
            foreach (var importModel in imports.Values)
            {
                if (Enum.TryParse<FormulaType>(importModel.ShortName, out var value))
                {
                    importModel.FormulaType = value;
                }
            }
        }
        
        #endregion

        
        #region ReferenceIntervals
        
        [HttpPost]
        [ApiRoute("Import/ReferenceIntervals")]
        public async Task<ImportReport> ImportReferenceIntervals()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                var report = new ImportReport();
                const int headerRow = 1;
                var metadataObject = new ImportReferenceIntervalMetadata(_indicatorService);
                
                try
                {
                    await using var stream = file.OpenReadStream();
                    var parseResult = _excelParser.Parse(stream);
                    var entityRows = parseResult
                        .Where(x => x.Key > headerRow)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var header = CreateAndValidateHeaderAndInitializeReport(metadataObject, parseResult, headerRow, entityRows.Count, report);

                    if (report.HeaderValid)
                    {
                        FillEmptyCells(header, entityRows);
                        ValidateCellsAndSetReportErrors(metadataObject, header, entityRows, report);

                        var importCandidates = CreateImportCandidates(metadataObject, header, entityRows, report);

                        ValidateEntitiesAndAddErrorsToReport(metadataObject, header, importCandidates, report);

                        if (!report.ErrorsByRows.Any())
                        {
                            ValidateReferenceIntervalsListAndAddErrorsToReport(header, importCandidates, report);
                        }
                        
                        if (!report.ErrorsByRows.Any())
                        {
                            var firstRowKeys = importCandidates[headerRow + 1].Values.Keys
                                .ToList();
                            
                            var indicators = await _indicatorService.GetIndicators(firstRowKeys);
                            var indicatorsDict = indicators.ToDictionary(x => x.ShortName, x => x, StringComparer.OrdinalIgnoreCase);
                            
                            var toInserts = importCandidates.Values
                                .Select(x => x.CreateEntity(indicatorsDict))
                                .ToList();

                            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                            
                            await _referenceIntervalService.DeleteAllReferenceIntervalValues();
                            await _referenceIntervalService.DeleteAllReferenceIntervalApplyCriteria();
                            
                            await SingleBulkInsertAndSetReport(_referenceIntervalService, toInserts, report);
                            
                            if (report.TotalInsertedErrorsCount == 0)
                            {
                                transaction.Complete();
                            }
                            else
                            {
                                transaction.Dispose();
                            }
                        }
                    }

                    stopwatch.Stop();

                    report.TotalExecutionTimeSeconds = stopwatch.ElapsedMilliseconds / 1000m;

                    report.CalculateReport();
                    report.BuildErrorsByColumns();
                }
                catch (Exception e)
                {
                    report.Error = e.Message;
                }
                
                return await Task.FromResult(report);
            }
            
            return null;
        }

        private static void ValidateReferenceIntervalsListAndAddErrorsToReport(Dictionary<string, string> header, Dictionary<int, ImportReferenceIntervalModel> imports, ImportReport report)
        {
            var entitiesGrouped = imports
                .GroupBy(x => x.Value.Sex);
            foreach (var group in entitiesGrouped)
            {
                var entities = group
                    .OrderBy(x => x.Value.AgeInterval.ValueMin)
                    .ToList();
                for (var i = 0; i < entities.Count - 1; i++)
                {
                    var currEntityPair = entities[i];
                    string message = null;
                    if (currEntityPair.Value.AgeInterval.ValueMax != entities[i + 1].Value.AgeInterval.ValueMin)
                    {
                        var sexStr = currEntityPair.Value.Sex == Sex.Male ? "М" : "Ж";
                        message =
                            $"Пол '{sexStr}': верхняя граница ({entities[i].Value.AgeInterval.ValueMax}) возрастного интервала не совпадает " +
                            $"с нижней границей ({entities[i + 1].Value.AgeInterval.ValueMin}) следующего интервала. Все интервалы должны составлять целый интекрвал без пропусков и пересечений.";
                    }

                    if (message != null)
                    {
                        if (!report.ErrorsByRows.ContainsKey(currEntityPair.Key))
                        {
                            report.ErrorsByRows[currEntityPair.Key] = new List<ColumnError>();
                        }
                        
                        report.ErrorsByRows[currEntityPair.Key].Add(new ColumnError{ Column = header["Возраст"], ErrorMessage = message });
                    }
                }
            }
        }

        #endregion
        
        
        #region Symptoms
        
        [HttpPost]
        [ApiRoute("Import/Symptoms")]
        public async Task<ImportReport> ImportSymptoms([FromForm]ImportSymptomForm model)
        {
            var file = Request.Form.Files.FirstOrDefault();
            var validation = await _importSymptomValidator.ValidateAsync(model);
            if (file != null && validation.IsValid)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                var report = new ImportReport();
                const int headerRowNumber = 1;
                
                try
                {
                    await using var stream = file.OpenReadStream();
                    var parseResult = _excelParser.Parse(stream);
                    var entityRows = parseResult
                        .Where(x => x.Key > headerRowNumber)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var symptomLevelCount = InitializeAndPreValidateSymptomsHeaderAndSetReport(parseResult, headerRowNumber, report);

                    var metadataObject = new ImportSymptomMetadata(symptomLevelCount, _indicatorService);
                    var header = CreateAndValidateHeaderAndInitializeReport(metadataObject, parseResult, headerRowNumber, entityRows.Count, report);

                    if (report.HeaderValid)
                    {
                        FillEmptyCells(header, entityRows);
                        ValidateCellsAndSetReportErrors(metadataObject, header, entityRows, report);

                        var importCandidates = CreateImportCandidates(metadataObject, header, entityRows, report);

                        await PrepareSymptomsForValidation(importCandidates, _deviationLevelService);
                        
                        ValidateEntitiesAndAddErrorsToReport(metadataObject, header, importCandidates, report);

                        if (!report.ErrorsByRows.Any())
                        {
                            ValidateSymptomsListAndAddErrorsToReport(header, importCandidates, report);
                        }
                        
                        if (!report.ErrorsByRows.Any())
                        {
                            var firstRowKeys = importCandidates
                                    .SelectMany(x => x.Value.DeviationLevels.Keys)
                                    .Distinct()
                                    .ToList();
                            
                            var indicators = await _indicatorService.GetIndicators(firstRowKeys);
                            var indicatorsDict = indicators.ToDictionary(x => x.ShortName, x => x);
                            var deviationLevels = await _deviationLevelService.GetAll();
                            var deviationLevelsDict = deviationLevels.ToDictionary(x => x.Alias, x => x, StringComparer.OrdinalIgnoreCase);
                            
                            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                            
                            if (model.SpecialistId == null)
                            {
                                var specialist = new Specialist
                                {
                                    ApplyToSexOnly = model.NewSpecialistSex,
                                    Name = model.NewSpecialistName
                                };
                                await _specialistService.CreateSpecialist(specialist);

                                model.SpecialistId = specialist.Id;
                            }

                            var specialistId = model.SpecialistId.Value;
                            var categoryId = model.SymptomCategoryId;
                            
                            var insertEntities = importCandidates.Values
                                .Select(x => Tuple.Create(x.CreateEntity(deviationLevelsDict, indicatorsDict, specialistId, categoryId), x.SymptomLevel))
                                .ToList();
                            var toInserts = BuildSymptomsTree(insertEntities);
                            
                            await _symptomService.DeleteAllSymptomIndicatorDeviationLevels(specialistId, categoryId);
                            await _symptomService.TryDeleteAllSymptoms(specialistId, categoryId);

                            await SingleBulkInsertAndSetReport(_symptomService, toInserts, report);
                            if (report.TotalInsertedErrorsCount == 0)
                            {
                                transaction.Complete();
                                report.TotalInsertedRows = insertEntities.Count;
                                report.Result = specialistId.ToString();
                            }
                            else
                            {
                                transaction.Dispose();
                            }
                        }
                    }

                    stopwatch.Stop();

                    report.TotalExecutionTimeSeconds = stopwatch.ElapsedMilliseconds / 1000m;

                    report.CalculateReport();
                    report.BuildErrorsByColumns();
                }
                catch (Exception e)
                {
                    report.Error = e.Message;
                }
                
                return await Task.FromResult(report);
            }

            return null;
        }

        private static int InitializeAndPreValidateSymptomsHeaderAndSetReport(Dictionary<int, Dictionary<string, Tuple<string, string>>> parseResult, int headerRowNumber, ImportReport report)
        {
            var symptomLevelCount = 0; 
            var headerRow = parseResult[headerRowNumber];
            if (headerRow.Any(x => x.Value.Item1 == "Болезни"))
            {
                var lastSymptomColumn = headerRow.First(x => x.Value.Item1 == "Болезни").Key[0];

                var i = 0;
                for (var column = 'A'; column <= lastSymptomColumn; column++)
                {
                    var columnStr = column.ToString();
                    var comment = string.Empty;
                    if (headerRow.TryGetValue(columnStr, out var cell))
                    {
                        comment = cell.Item2;
                    }
                    headerRow[columnStr] = Tuple.Create($"Болезнь{i + 1}", comment);
                    
                    if (column == lastSymptomColumn)
                    {
                        symptomLevelCount = i + 1;
                        break;
                    }

                    i++;
                }
            }
            else
            {
                report.HeaderErrors.Add("Столбец 'Болезни' отсутствует в заголовке.");
            }

            return symptomLevelCount;
        }

        private static async Task PrepareSymptomsForValidation(Dictionary<int, ImportSymptomModel> importCandidates, IDeviationLevelService deviationLevelService)
        {
            var deviationLevels = await deviationLevelService.GetAll();
            var aliases = deviationLevels.Select(x => x.Alias).ToHashSet();
            aliases.Add(string.Empty);
            aliases.Add(null);
            aliases.Remove("N");
            
            foreach (var symptomModel in importCandidates)
            {
                foreach (var deviationLevel in symptomModel.Value.DeviationLevels)
                {
                    deviationLevel.Value.AllowedAliases = aliases;
                }

                if (int.TryParse(symptomModel.Value.SymptomLevelStr?.Replace("Болезнь", ""), out var level))
                {
                    symptomModel.Value.SymptomLevel = level;
                }
            }
        }

        private static void ValidateSymptomsListAndAddErrorsToReport(Dictionary<string, string> header, Dictionary<int, ImportSymptomModel> imports, ImportReport report)
        {
            var entitiesList = imports
                .OrderBy(x => x.Key)
                .ToList();

            var currEntityPair = entitiesList[0];
            var prevLevel = currEntityPair.Value.SymptomLevel;
            string message = null;
            if (prevLevel > 1)
            {
                message = $"Первая строка должна быть диагнозом уровня 1. Текущий уровень: ({prevLevel}).";
            }
            AddErrorByRow(header, currEntityPair.Key, message, currEntityPair.Value.SymptomLevelStr, report);
            
            for (var i = 1; i < imports.Count; i++)
            {
                currEntityPair = entitiesList[i];
                var currLevel = currEntityPair.Value.SymptomLevel;
                message = null;
                if (currLevel > prevLevel + 1)
                {
                    message =
                        $"Уровень симптома ({currLevel}) больше уровня предыдушего симптома/диагноза ({prevLevel}) больше чем на 1";
                }

                AddErrorByRow(header, currEntityPair.Key, message, currEntityPair.Value.SymptomLevelStr, report);

                prevLevel = currLevel;
            }
        }

        private static List<Symptom> BuildSymptomsTree(List<Tuple<Symptom, int>> symptoms)
        {
            var result = new List<Symptom>();
            var maxLevel = 1;
            var lastLevelIndexes = new Dictionary<int, int>();
            var lastLevelSex = new Dictionary<int, Sex?>();
            for (var i = 0; i < symptoms.Count; i++)
            {
                var (symptom, currLevel) = symptoms[i];

                maxLevel = Math.Max(currLevel, maxLevel);
                if (currLevel == 1)
                {
                    result.Add(symptom);
                }
                else
                {
                    var parentIndex = lastLevelIndexes[currLevel - 1];
                    var parentSex = lastLevelSex[currLevel - 1];
                    if (parentSex != null)
                    {
                        symptom.ApplyToSexOnly = parentSex.Value; // overwrites sex of lower level symptoms with parent, if specified
                    }
                    
                    symptoms[parentIndex].Item1.Children ??= new List<Symptom>();
                    symptoms[parentIndex].Item1.Children.Add(symptom);
                }

                lastLevelIndexes[currLevel] = i;
                lastLevelSex[currLevel] = symptom.ApplyToSexOnly;
                
                for (var l = currLevel + 1; l <= maxLevel; l++)
                {
                    lastLevelSex.Remove(l);
                    lastLevelIndexes.Remove(l);
                }
            }

            return result;
        }

        [HttpGet]
        [ApiRoute("Import/Lists/Lookups")]
        public async Task<Dictionary<string, List<LookupModel>>> GetLookups()
        {
            var result = new Dictionary<string, List<LookupModel>>();
            
            var specialists = await _specialistService.GetSpecialists();
            var model = specialists.Select(x => new LookupModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            result["Specialists"] = model;

            var categories = await _symptomCategoryService.GetAll();
            model = categories.Select(x => new LookupModel
            {
                Id = x.Id,
                Name = x.DisplayName
            }).ToList();

            result["Categories"] = model;

            model = new List<LookupModel>
            {
                new() {Id = 0, Name = "Любой"},
                new() {Id = (int) Sex.Male, Name = "Мужской"},
                new() {Id = (int) Sex.Female, Name = "Женский"}
            };

            result["Sex"] = model;

            return result;
        }

        #endregion


        #region Analysis

        [HttpPost]
        [ApiRoute("Import/Analysis")]
        public async Task<ImportReport> ImportAnalysis([FromQuery] int? specialistId)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                var report = new ImportReport();
                const int headerRow = 1;
                var metadataObjectAnalysis = new ImportAnalysisMetadata();
                var metadataObjectIndicator = new ImportAnalysisIndicatorMetadata();
                var metadataObjectDeviationLevel = new ImportAnalysisDeviationLevelMetadata();
                
                try
                {
                    await using var stream = file.OpenReadStream();
                    var parseResult = _excelParser.Parse(stream);
                    var entityRows = parseResult
                        .Where(x => x.Key > headerRow)
                        .ToDictionary(x => x.Key, x => x.Value);

                    var header = CreateAndValidateHeaderAndInitializeReport(metadataObjectIndicator, parseResult, headerRow, entityRows.Count, report);
                    CreateAndValidateHeaderAndInitializeReport(metadataObjectAnalysis, parseResult, headerRow, entityRows.Count, report);
                    CreateAndValidateHeaderAndInitializeReport(metadataObjectDeviationLevel, parseResult, headerRow, entityRows.Count, report, true);

                    if (report.HeaderValid)
                    {
                        FillEmptyCells(header, entityRows);
                        
                        ValidateCellsAndSetReportErrors(metadataObjectAnalysis, header, entityRows, report);
                        ValidateCellsAndSetReportErrors(metadataObjectIndicator, header, entityRows, report);
                        if (report.HeaderWithoutWarnings)
                        {
                            ValidateCellsAndSetReportErrors(metadataObjectDeviationLevel, header, entityRows, report);    
                        }

                        var importCandidatesAnalysis = CreateImportCandidates(metadataObjectAnalysis, header, entityRows, report)
                            .Where(x => x.Value.Sex != null)
                            .ToDictionary(x => x.Key, x => x.Value);
                        var importCandidatesIndicator = CreateImportCandidates(metadataObjectIndicator, header, entityRows, report)
                            .Where(x => x.Value.Value != null)
                            .ToDictionary(x => x.Key, x => x.Value);
                        var importCandidatesDeviationLevel = new Dictionary<int, ImportAnalysisDeviationLevelModel>();
                        if (report.HeaderWithoutWarnings)
                        {
                            importCandidatesDeviationLevel = CreateImportCandidates(metadataObjectDeviationLevel, header, entityRows, report)
                                .Where(x => x.Value.Alias != null)
                                .ToDictionary(x => x.Key, x => x.Value);
                        }
                        
                        var indicators = await _indicatorService.GetAnalysisIndicators();
                        var deviationLevels = await _deviationLevelService.GetAll();
                        
                        PrepareAnalysisIndicators(importCandidatesIndicator, indicators);
                        if (report.HeaderWithoutWarnings)
                        {
                            PrepareAnalysisDeviationLevels(importCandidatesDeviationLevel, deviationLevels);
                        }

                        ValidateEntitiesAndAddErrorsToReport(metadataObjectAnalysis, header, importCandidatesAnalysis, report);
                        ValidateEntitiesAndAddErrorsToReport(metadataObjectIndicator, header, importCandidatesIndicator, report);
                        if (report.HeaderWithoutWarnings)
                        {
                            ValidateEntitiesAndAddErrorsToReport(metadataObjectDeviationLevel, header, importCandidatesDeviationLevel, report);
                        }

                        if (!report.ErrorsByRows.Any())
                        {
                            if (report.HeaderWithoutWarnings)
                            {
                                ValidateAnalysisDeviationLevelsListAndAddErrorsToReport(header, importCandidatesDeviationLevel, report);
                            }
                            ValidateAnalysisListAndAddErrorsToReport(header, importCandidatesAnalysis, report);
                        }
                        
                        if (!report.ErrorsByRows.Any())
                        {
                            var toInsertsAnalysis = importCandidatesAnalysis.Values
                                .Select(x => x.CreateEntity())
                                .First();
                            var toInsertIndicators = importCandidatesIndicator.Values
                                .Select(x => x.CreateEntity())
                                .ToList();
                            var toInsertDeviationLevels = new List<AnalysisDeviationLevel>();
                            if (report.HeaderWithoutWarnings)
                            {
                                toInsertDeviationLevels = importCandidatesDeviationLevel.Values
                                    .Select(x => x.CreateEntity(deviationLevels))
                                    .ToList();
                            }

                            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                            await SingleInsertAndSetReport(_analysisService, toInsertsAnalysis, report);

                            foreach (var indicator in toInsertIndicators)
                            {
                                indicator.Analysis = toInsertsAnalysis;
                            }
                            foreach (var deviationLevel in toInsertDeviationLevels)
                            {
                                deviationLevel.Analysis = toInsertsAnalysis;
                            }
                            
                            await SingleBulkInsertAndSetReport(_analysisIndicatorService, toInsertIndicators, report);
                            await SingleBulkInsertAndSetReport(_deviationLevelService, toInsertDeviationLevels, report);
                            
                            if (report.TotalInsertedErrorsCount == 0)
                            {
                                transaction.Complete();
                            }
                            else
                            {
                                transaction.Dispose();
                            }
                        }
                    }

                    stopwatch.Stop();

                    report.TotalExecutionTimeSeconds = stopwatch.ElapsedMilliseconds / 1000m;

                    report.CalculateReport();
                    report.BuildErrorsByColumns();
                }
                catch (Exception e)
                {
                    report.Error = e.Message;
                }
                
                return await Task.FromResult(report);
            }
            
            return null;
        }

        private static void PrepareAnalysisIndicators(Dictionary<int, ImportAnalysisIndicatorModel> importCandidates, List<Indicator> indicators)
        {
            var indicatorsDict = indicators.ToDictionary(x => x.ShortName, x => x);
            
            foreach (var (_, analysisIndicatorModel) in importCandidates)
            {
                
                analysisIndicatorModel.AllowedIndicatorShortNames = indicatorsDict.Keys.ToList();
                
                if (indicatorsDict.TryGetValue(analysisIndicatorModel.Indicator, out var indicator))
                {
                    analysisIndicatorModel.IndicatorId = indicator.Id;
                    analysisIndicatorModel.Calculated = indicator.FormulaType.HasValue;
                }
            }
        }

        private static void PrepareAnalysisDeviationLevels(Dictionary<int, ImportAnalysisDeviationLevelModel> importCandidates, List<DeviationLevel> deviationLevels)
        {
            var deviationLevelsDict = deviationLevels.ToDictionary(x => x.Alias, x => x);
            
            foreach (var (_, analysisDeviationLevelModel) in importCandidates)
            {
                analysisDeviationLevelModel.AllowedAliases = deviationLevelsDict.Keys.ToList();
                analysisDeviationLevelModel.AllowedDeviationLevelIds = deviationLevels.Select(x => x.Id).ToList();
                
                if (deviationLevelsDict.TryGetValue(analysisDeviationLevelModel.Alias, out var deviationLevel))
                {
                    analysisDeviationLevelModel.DeviationLevelId = deviationLevel.Id;
                }
            }
        }

        private static void ValidateAnalysisDeviationLevelsListAndAddErrorsToReport(Dictionary<string, string> header, Dictionary<int, ImportAnalysisDeviationLevelModel> imports, ImportReport report)
        {
            var orderedImports = imports.OrderBy(x => x.Value.DeviationLevelId).ToList();
            for (var i = 0; i < orderedImports.Count - 1; i++)
            {
                var (currKey, currValue) = orderedImports[i];
                var (_, nextValue) = orderedImports[i + 1];

                string message = null;
                
                if (currValue.MinPercentFromCenter != null &&
                    nextValue.MinPercentFromCenter != null)
                {
                    if (currValue.MinPercentFromCenter <= nextValue.MinPercentFromCenter)
                    {
                        message =
                            $"Значение вниз от центра ({currValue.MinPercentFromCenter}) для уровня отклонения '{currValue.Alias}'" +
                            $" должно быть больше значения вниз от центра ({nextValue.MinPercentFromCenter}) для уровня отклонения '{nextValue.Alias}'";
                    }
                }

                AddErrorByRow(header, currKey, message, "Вниз от центра%", report);

                message = null;
                
                if (currValue.MaxPercentFromCenter != null &&
                    nextValue.MaxPercentFromCenter != null)
                {
                    if (currValue.MaxPercentFromCenter >= nextValue.MaxPercentFromCenter)
                    {
                        message =
                            $"Значение вверх от центра ({currValue.MaxPercentFromCenter}) для уровня отклонения '{currValue.Alias}'" +
                            $" должно быть меньше значения вниз от центра ({nextValue.MaxPercentFromCenter}) для уровня отклонения '{nextValue.Alias}'";
                    }
                }
                
                AddErrorByRow(header, currKey, message, "Вверх от центра%", report);
            }
        }

        private static void ValidateAnalysisListAndAddErrorsToReport(Dictionary<string, string> header, Dictionary<int, ImportAnalysisModel> imports, ImportReport report)
        {
            foreach (var currEntityPair in imports.Where(x => x.Key > imports.Keys.Min()))
            {
                var message = $"Недопустимо больше одной строки данных анализа";
                AddErrorByRow(header, currEntityPair.Key, message, "Пол", report);
            }
        }

        #endregion
        
        
        #region Common Methods

        private static Dictionary<string, string> CreateAndValidateHeaderAndInitializeReport<TImport>(BaseMetadata<TImport> metadataObject, Dictionary<int, Dictionary<string, Tuple<string, string>>> parseResult, int headerRow, int entityRowsCount, ImportReport report, bool setWarningsInsteadOfErrors = false)
            where TImport: new()
        {
            var headerItems = parseResult[headerRow]
                .Where(x => !string.IsNullOrEmpty(x.Value.Item1))
                .Select(x => new KeyValuePair<string, string>(x.Value.Item1, x.Key))
                .ToList();
            
            report.TotalRowsFound = entityRowsCount;

            if (setWarningsInsteadOfErrors)
            {
                report.HeaderWarnings.AddRange(metadataObject.ValidateHeader(headerItems));
            }
            else
            {
                report.HeaderErrors.AddRange(metadataObject.ValidateHeader(headerItems));                
            }
            
            if (!setWarningsInsteadOfErrors && !report.HeaderValid || !report.HeaderWithoutWarnings) return null;
            
            var header = headerItems.ToDictionary(
                x => x.Key,
                x => x.Value);

            report.ColumnNames = headerItems.ToDictionary(
                x => x.Value,
                x => x.Key);
            return header;
        }

        private static void FillEmptyCells(Dictionary<string, string> header, Dictionary<int, Dictionary<string, Tuple<string, string>>> entityRows)
        {
            foreach (var (_, rowDict) in entityRows)
            {
                foreach (var column in header.Values.Where(column => !rowDict.ContainsKey(column)))
                {
                    rowDict[column] = new Tuple<string, string>(string.Empty, string.Empty);
                }
            }
        }
        
        private static void ValidateCellsAndSetReportErrors<TImport>(BaseMetadata<TImport> metadataObject, Dictionary<string, string> header, Dictionary<int, Dictionary<string, Tuple<string, string>>> entityRows, ImportReport report)
            where TImport: new()
        {
            var errorsByRows = entityRows
                .Select(x => Tuple.Create(
                    x.Key,
                    metadataObject
                        .ValidateCells(header, x.Value)
                        .Select(y => new ColumnError
                        {
                            Column = y.Key,
                            ErrorMessage = y.Value
                        })
                        .Where(y => y.ErrorMessage != null)
                        .ToList())
                )
                .Where(x => x.Item2.Any())
                .ToDictionary(x => x.Item1, x => x.Item2);

            foreach (var (row, columnErrors) in errorsByRows)
            {
                if (!report.ErrorsByRows.ContainsKey(row))
                {
                    report.ErrorsByRows[row] = new List<ColumnError>();
                }
                
                report.ErrorsByRows[row].AddRange(columnErrors);
            }
        }

        private static Dictionary<int, TImport> CreateImportCandidates<TImport>(BaseMetadata<TImport> metadataObject, Dictionary<string, string> header, Dictionary<int, Dictionary<string, Tuple<string, string>>> entityRows, ImportReport report)
            where TImport: new()
        {
            return entityRows
                .Where(x => !report.ErrorsByRows.Keys.Contains(x.Key))
                .Select(x => Tuple.Create(x.Key, metadataObject.CreateEntity(header, x.Value)))
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        private static void ValidateEntitiesAndAddErrorsToReport<TImport>(BaseMetadata<TImport> metadataObject, Dictionary<string, string> header, Dictionary<int, TImport> imports, ImportReport report)
            where TImport: new()
        {
            foreach (var (row, importModel) in imports)
            {
                var validationResult = metadataObject.ValidateEntity(header, importModel);
                if (!validationResult.Any()) continue;
                
                if (!report.ErrorsByRows.ContainsKey(row))
                {
                    report.ErrorsByRows[row] = new List<ColumnError>();
                }

                report.ErrorsByRows[row].AddRange(validationResult.SelectMany(x => x
                    .Value.Select(y => new ColumnError
                    {
                        Column = x.Key,
                        ErrorMessage = y
                    })));
            }
        }
        
        private static void AddErrorByRow(IReadOnlyDictionary<string, string> header, int row, string message, string columnName, ImportReport report)
        {
            if (message == null) return;
            
            if (!report.ErrorsByRows.ContainsKey(row))
            {
                report.ErrorsByRows[row] = new List<ColumnError>();
            }
            
            report.ErrorsByRows[row].Add(new ColumnError{ Column = header[columnName], ErrorMessage = message });
        }

        private static async Task SingleBulkUpdateAndSetReport<TImportEntity>(IImportService<TImportEntity> importService, List<TImportEntity> toUpdates, ImportReport report)
        {
            try
            {
                await importService.UpdateBulk(toUpdates);
                report.TotalUpdatedRows += toUpdates.Count;
            }
            catch (Exception)
            {
                report.TotalUpdatedErrorsCount += toUpdates.Count;
            }
        }

        private static async Task SingleBulkInsertAndSetReport<TImportEntity>(IImportService<TImportEntity> importService, List<TImportEntity> toInserts, ImportReport report)
        {
            try
            {
                await importService.InsertBulk(toInserts);
                report.TotalInsertedRows += toInserts.Count;
            }
            catch (Exception e)
            {
                report.TotalInsertedErrorsCount += toInserts.Count;
            }
        }

        private static async Task SingleInsertAndSetReport<TImportEntity>(IImportService<TImportEntity> importService,
            TImportEntity toInsert, ImportReport report)
        {
            try
            {
                await importService.Insert(toInsert);
                report.TotalInsertedRows += 1;
            }
            catch (Exception)
            {
                report.TotalInsertedErrorsCount += 1;
            }
        }
        
        #endregion
        
        
        #region Fake Report Data
        
        // private static ImportReport FakeData()
        // {
        //     var result = new ImportReport
        //     {
        //         HeaderValid = true,
        //         TotalRowsFound = 33,
        //         TotalRowsReady = 30,
        //         CountInvalidRows = 3,
        //         TotalInsertedRows = 20,
        //         TotalUpdatedRows = 10,
        //         TotalInsertedErrorsCount = 0,
        //         TotalUpdatedErrorsCount = 0,
        //         TotalExecutionTimeSeconds = 1.111m,
        //         
        //         CountErrors = 6,
        //         ColumnNames = new Dictionary<string, string>
        //         {
        //             {"A", "Активный"}, {"B", "Название"}
        //         },
        //         ErrorsByRows = new Dictionary<int, List<ColumnError>>
        //         {
        //             {
        //                 1, new List<ColumnError>
        //                 {
        //                     new() { Column = "A", ErrorMessage = "AAAAAAAAAAAAAAA"},
        //                     new() { Column = "B", ErrorMessage = "B B B B B B B B"}
        //                 }
        //             },
        //             {
        //                 2, new List<ColumnError>
        //                 {
        //                     new() { Column = "A", ErrorMessage = "QQQQQQQ"},
        //                     new() { Column = "B", ErrorMessage = "QQQ B B B B"}
        //                 }
        //             },
        //             {
        //                 3, new List<ColumnError>
        //                 {
        //                     new() { Column = "A", ErrorMessage = "TTTTT"},
        //                     new() { Column = "B", ErrorMessage = "QQQ B B B B"}
        //                 }
        //             },
        //         }
        //     };
        //     
        //     result.BuildErrorsByColumns();
        //     return result;
        // }

        #endregion
    }
}