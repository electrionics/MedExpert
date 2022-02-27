import {Component, Inject} from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-import-reference-intervals',
  templateUrl: './importReferenceIntervals.component.html'
})
export class ImportReferenceIntervalsComponent {
  public Options: ImportOptions;
  public Report: ImportReport;

  private readonly http: HttpClient;
  private readonly baseUrl: string;

  public fileToUpload: File;
  public requestProgress: boolean;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string) {
    this.http = http;
    this.baseUrl = baseUrl;

    this.requestProgress = false;
  }

  public changeFile(files: FileList){
    this.fileToUpload = files.item(0);
  }

  public upload(){
    if (this.fileToUpload){
      let formData = new FormData();
      formData.append('import', this.fileToUpload, this.fileToUpload.name);
      this.requestProgress = true;
      this.http.post<ImportReport>(this.baseUrl + 'Import/ReferenceInterval', formData).subscribe(result => {
        this.Report = result;

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
      }, error => console.error(error), () => this.requestProgress = false );
    }
  }

  public toggleErrorsByColumns(){
    this.Options.showErrorsByColumns = !this.Options.showErrorsByColumns;
  }

  public toggleExpanded(){
    this.Options.errorsExpanded = !this.Options.errorsExpanded;
    for (let row in this.Options.expandedRows){
      this.Options.expandedRows[row] = this.Options.errorsExpanded;
    }
    for (let column in this.Options.expandedColumns){
      this.Options.expandedColumns[column] = this.Options.errorsExpanded;
    }
  }

  public toggleColumn(column){
    this.Options.expandedColumns[column] = !this.Options.expandedColumns[column];
  }

  public toggleRow(row){
    this.Options.expandedRows[row] = !this.Options.expandedRows[row];
  }
}

class ImportOptions{
  showErrorsByColumns: boolean;
  errorsExpanded: boolean;
  expandedRows: {[row: number]: boolean };
  expandedColumns: {[column: string]: boolean };
}

class ImportReport{
  headerValid: boolean;
  totalRowsFound: number;
  totalRowsReady: number;
  countInvalidRows: number;
  totalInsertedRows: number;
  totalUpdatedRows: number;
  totalInsertedErrorsCount: number;
  totalUpdatedErrorsCount: number;
  totalExecutionTimeSeconds: number;

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
