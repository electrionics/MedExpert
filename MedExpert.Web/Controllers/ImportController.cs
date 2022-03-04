using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Excel;
using MedExpert.Excel.Metadata;
using MedExpert.Excel.Model;
using Microsoft.AspNetCore.Mvc;
using MedExpert.Web.ViewModels;
using MedExpert.Services.Interfaces;

// ReSharper disable StringLiteralTypo

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class ImportController:ControllerBase
    {
        private readonly IReferenceIntervalService _referenceIntervalService;
        private readonly IIndicatorService _indicatorService;
        private readonly ExcelParser _excelParser;

        public ImportController(IReferenceIntervalService referenceIntervalService, ExcelParser excelParser, IIndicatorService indicatorService)
        {
            _referenceIntervalService = referenceIntervalService;
            _excelParser = excelParser;
            _indicatorService = indicatorService;
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
                            
                            transaction.Complete();
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
        public Task<ImportReport> ImportSymptoms()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var result = _excelParser.Parse(stream);
            }
            
            return Task.FromResult(FakeData());
        }

        #endregion
        
        
        #region Common Methods

        private static Dictionary<string, string> CreateAndValidateHeaderAndInitializeReport<TImport>(BaseMetadata<TImport> metadataObject, Dictionary<int, Dictionary<string, Tuple<string, string>>> parseResult, int headerRow, int entityRowsCount, ImportReport report)
            where TImport: new()
        {
            var header = parseResult[headerRow].ToDictionary(
                x => x.Value.Item1,
                x => x.Key);
            
            report.HeaderValid = metadataObject.ValidateHeader(header);
            report.TotalRowsFound = entityRowsCount;
            report.ColumnNames = header.ToDictionary(x => x.Value, x => x.Key);

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
            report.ErrorsByRows = entityRows
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
            foreach (var importModel in imports)
            {
                var validationResult = metadataObject.ValidateEntity(header, importModel.Value);
                if (!validationResult.Any()) continue;
                
                if (!report.ErrorsByRows.ContainsKey(importModel.Key))
                {
                    report.ErrorsByRows[importModel.Key] = new List<ColumnError>();
                }

                report.ErrorsByRows[importModel.Key].AddRange(validationResult.SelectMany(x => x
                    .Value.Select(y => new ColumnError
                    {
                        Column = x.Key,
                        ErrorMessage = y
                    })));
            }
        }

        public static async Task SingleBulkUpdateAndSetReport<TImportEntity>(IImportService<TImportEntity> importService, List<TImportEntity> toUpdates, ImportReport report)
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
        
        public static async Task SingleBulkInsertAndSetReport<TImportEntity>(IImportService<TImportEntity> importService, List<TImportEntity> toInserts, ImportReport report)
        {
            try
            {
                await importService.InsertBulk(toInserts);
                report.TotalInsertedRows = toInserts.Count;
            }
            catch (Exception)
            {
                report.TotalInsertedErrorsCount = toInserts.Count;
            }
        }
        
        #endregion
        

        private static ImportReport FakeData()
        {
            var result = new ImportReport
            {
                HeaderValid = true,
                TotalRowsFound = 33,
                TotalRowsReady = 30,
                CountInvalidRows = 3,
                TotalInsertedRows = 20,
                TotalUpdatedRows = 10,
                TotalInsertedErrorsCount = 0,
                TotalUpdatedErrorsCount = 0,
                TotalExecutionTimeSeconds = 1.111m,
                
                CountErrors = 6,
                ColumnNames = new Dictionary<string, string>
                {
                    {"A", "Активный"}, {"B", "Название"}
                },
                ErrorsByRows = new Dictionary<int, List<ColumnError>>
                {
                    {
                        1, new List<ColumnError>
                        {
                            new() { Column = "A", ErrorMessage = "AAAAAAAAAAAAAAA"},
                            new() { Column = "B", ErrorMessage = "B B B B B B B B"}
                        }
                    },
                    {
                        2, new List<ColumnError>
                        {
                            new() { Column = "A", ErrorMessage = "QQQQQQQ"},
                            new() { Column = "B", ErrorMessage = "QQQ B B B B"}
                        }
                    },
                    {
                        3, new List<ColumnError>
                        {
                            new() { Column = "A", ErrorMessage = "TTTTT"},
                            new() { Column = "B", ErrorMessage = "QQQ B B B B"}
                        }
                    },
                }
            };
            
            result.BuildErrorsByColumns();
            return result;
        }
    }
}