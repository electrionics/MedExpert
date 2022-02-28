import {Component, Inject, Input} from '@angular/core';
import {ImportOptions, ImportReport} from "./importBase";

@Component({
  selector: 'app-import-report',
  templateUrl: './importReport.component.html'
})
export class ImportReportComponent {
  @Input()
  public Options: ImportOptions;
  @Input()
  public Report: ImportReport;

  constructor() {
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


