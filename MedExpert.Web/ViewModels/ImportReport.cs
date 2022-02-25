using System.Collections.Generic;

namespace MedExpert.Web.ViewModels
{
    public class ImportReport
    {
        public bool HeaderValid { get; set; }
        public int TotalRowsFound { get; set; }
        public int TotalRowsReady { get; set; }
        public int CountInvalidRows { get; set; }
        public int TotalInsertedRows { get; set; }
        public int TotalUpdatedRows { get; set; }
        public int TotalInsertedErrorsCount { get; set; }
        public int TotalUpdatedErrorsCount { get; set; }
        public decimal TotalExecutionTimeSeconds { get; set; }

        public int CountErrors { get; set; }
        public Dictionary<int, List<ColumnError>> ErrorsByRows { get; set; }
        public Dictionary<string, List<RowError>> ErrorsByColumns { get; set; }
        public Dictionary<string, string> ColumnNames { get; set; }
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