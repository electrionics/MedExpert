using System;
using System.Collections.Generic;
using System.Linq;

namespace MedExpert.Web.ViewModels
{
    public class ImportReport
    {
        public ImportReport()
        {
            HeaderErrors = new List<string>();
            ErrorsByRows = new Dictionary<int, List<ColumnError>>();
        }

        public bool HeaderValid => !HeaderErrors.Any();
        public List<string> HeaderErrors { get; set; }
        public int TotalRowsFound { get; set; }
        public int TotalRowsReady { get; set; }
        public int CountInvalidRows { get; set; }
        public int TotalInsertedRows { get; set; }
        public int TotalUpdatedRows { get; set; }
        public int TotalInsertedErrorsCount { get; set; }
        public int TotalUpdatedErrorsCount { get; set; }
        public decimal TotalExecutionTimeSeconds { get; set; }
        
        public string Error { get; set; }
        
        public string Result { get; set; }

        public int CountErrors { get; set; }
        public Dictionary<int, List<ColumnError>> ErrorsByRows { get; set; }
        public Dictionary<string, List<RowError>> ErrorsByColumns { get; set; }
        public Dictionary<string, string> ColumnNames { get; set; }

        public void CalculateReport()
        {
            TotalRowsReady = TotalRowsFound - ErrorsByRows.Count;
            CountInvalidRows = ErrorsByRows.Count;
            CountErrors = ErrorsByRows.Sum(x => x.Value.Count);
        }
        
        public void BuildErrorsByColumns()
        {
            ErrorsByColumns = ErrorsByRows.SelectMany(x => x.Value
                    .Select(y => Tuple.Create(y.Column, new RowError
                    {
                        Row = x.Key,
                        ErrorMessage = y.ErrorMessage
                    }))
                    .ToList()
                )
                .GroupBy(x => x.Item1)
                .ToDictionary(x => x.Key, x => x
                    .Select(y => y.Item2)
                    .ToList()
                );
        }
    }

    public class RowError
    {
        public int Row { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class ColumnError{
        public string Column { get; set; }
        public string ErrorMessage { get; set; }
    }
}