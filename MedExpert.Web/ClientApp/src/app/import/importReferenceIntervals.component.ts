import {Component, Inject} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {ImportBaseComponent, ImportOptions, ImportReport} from "./importBase.component";

@Component({
  selector: 'app-import-reference-intervals',
  templateUrl: './importReferenceIntervals.component.html'
})
export class ImportReferenceIntervalsComponent extends ImportBaseComponent{
  private readonly http: HttpClient;
  private readonly baseUrl: string;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string) {
    super();

    this.http = http;
    this.baseUrl = baseUrl;
  }

  public upload(){
    if (this.fileToUpload){
      let formData = new FormData();
      formData.append('import', this.fileToUpload, this.fileToUpload.name);
      this.requestProgress = true;
      this.http.post<ImportReport>(this.baseUrl + 'Import/ReferenceInterval', formData).subscribe(result => {
        this.setupReportAndOptions(result);
      }, error => console.error(error), () => this.requestProgress = false );
    }
  }
}
