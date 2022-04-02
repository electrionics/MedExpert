export class ImportBaseComponent {
  public fileToUpload: File;
  public requestProgress: boolean;

  public Options: ImportOptions;
  public Report: ImportReport;

  constructor() {
    this.requestProgress = false;
  }

  public changeFile(files: FileList){
    this.fileToUpload = files?.item(0);
  }

  protected setupReportAndOptions(result){
    this.Report = result;

    if (result){
      this.Options = new ImportOptions();
      this.Options.showErrorsByColumns = false;
      this.Options.errorsExpanded = false;
      this.Options.expandedColumns = {};
      this.Options.expandedRows = {};
      for (let column in this.Report.errorsByColumns){
        this.Options.expandedColumns[column] = false;
      }
      for (let row in this.Report.errorsByRows){
        this.Options.expandedRows[row] = false;
      }
    }
  }
}

export class ImportOptions{
  showErrorsByColumns: boolean;
  errorsExpanded: boolean;
  expandedRows: {[row: number]: boolean };
  expandedColumns: {[column: string]: boolean };
}

export class ImportReport{
  headerValid: boolean;
  headerErrors: string[];
  totalRowsFound: number;
  totalRowsReady: number;
  countInvalidRows: number;
  totalInsertedRows: number;
  totalUpdatedRows: number;
  totalInsertedErrorsCount: number;
  totalUpdatedErrorsCount: number;
  totalExecutionTimeSeconds: number;
  error: string;
  result: string;

  countErrors: number;
  errorsByRows: { [row: number]: ColumnError[] };
  errorsByColumns: { [column: string]: RowError[] };
  columnNames: { [column: string]: string }
}

class RowError{
  row: number;
  errorMessage: string;
}

class ColumnError{
  column: string;
  errorMessage: string;
}
