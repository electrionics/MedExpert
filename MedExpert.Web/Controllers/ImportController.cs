using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedExpert.Domain.Entities;
using MedExpert.Domain.Enums;
using MedExpert.Excel;
using Microsoft.AspNetCore.Mvc;
using MedExpert.Web.ViewModels;
using MedExpert.Services.Interfaces;

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class ImportController:ControllerBase
    {
        private readonly IReferenceIntervalService _referenceIntervalService;
        private readonly ExcelParser _excelParser;

        public ImportController(IReferenceIntervalService referenceIntervalService, ExcelParser excelParser)
        {
            _referenceIntervalService = referenceIntervalService;
            _excelParser = excelParser;
        }
        
        #region Indicators

        [HttpPost]
        [ApiRoute("Import/Indicators")]
        public Task<ImportReport> ImportIndicators()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                var result = _excelParser.Parse(stream);
            }
            
            var indicators = new List<Indicator>();

            foreach (var indicator in indicators)
            {
                if (Enum.TryParse<FormulaType>(indicator.ShortName, out var value))
                {
                    indicator.FormulaType = value;
                }
            }
            
            return Task.FromResult(FakeData());
        }
        
        #endregion

        #region ReferenceIntervals
        
        [HttpPost]
        [ApiRoute("Import/ReferenceIntervals")]
        public Task<ImportReport> ImportReferenceIntervals()
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

        private static ImportReport FakeData()
        {
            return new ImportReport
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
                            new ColumnError{ Column = "A", ErrorMessage = "AAAAAAAAAAAAAAA"},
                            new ColumnError{ Column = "B", ErrorMessage = "B B B B B B B B"}
                        }
                    },
                    {
                        2, new List<ColumnError>
                        {
                            new ColumnError{ Column = "A", ErrorMessage = "QQQQQQQ"},
                            new ColumnError{ Column = "B", ErrorMessage = "QQQ B B B B"}
                        }
                    },
                    {
                        3, new List<ColumnError>
                        {
                            new ColumnError{ Column = "A", ErrorMessage = "TTTTT"},
                            new ColumnError{ Column = "B", ErrorMessage = "QQQ B B B B"}
                        }
                    },
                },
                ErrorsByColumns = new Dictionary<string, List<RowError>>
                {
                    {
                        "A", new List<RowError>
                        {
                            new RowError{ Row = 1, ErrorMessage = "AAAAAAAAAAAAAAA"},
                            new RowError{ Row = 2, ErrorMessage = "QQQQQQQ"},
                            new RowError{ Row = 3, ErrorMessage = "TTTTT"}
                        }
                    },
                    {
                        "B", new List<RowError>
                        {
                            new RowError{ Row = 1, ErrorMessage = "B B B B B B B B"},
                            new RowError{ Row = 2, ErrorMessage = "QQQ B B B B"},
                            new RowError{ Row = 3, ErrorMessage = "QQQ B B B B"}
                        }
                    }
                }
            };
        }
    }
}