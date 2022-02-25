using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MedExpert.Web.ViewModels;
using MedExpert.Services.Interfaces;

namespace MedExpert.Web.Controllers
{
    [ApiController]
    public class ImportController:ControllerBase
    {
        private readonly IReferenceIntervalService _referenceIntervalService;


        public ImportController(IReferenceIntervalService referenceIntervalService)
        {
            _referenceIntervalService = referenceIntervalService;
        }

        #region ReferenceInterval
        
        [HttpPost]
        [ApiRoute("Import/ReferenceInterval")]
        public Task<ImportReport> ImportReferenceInterval()
        {
            return Task.FromResult(FakeData());
        }

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

        #endregion
    }
}