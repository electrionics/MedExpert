import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {ImportBaseComponent, ImportReport} from "./importBase";

@Component({
  selector: 'app-import-symptoms',
  templateUrl: './importSymptoms.component.html'
})
export class ImportSymptomsComponent extends ImportBaseComponent{
  private readonly http: HttpClient;
  private readonly baseUrl: string;

  public specialists: Specialist[];
  public specialistId: number;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string) {
    super();

    this.http = http;
    this.baseUrl = baseUrl;
  }

  ngOnInit(){
    this.requestProgress = true;
    this.http.get<Specialist[]>(this.baseUrl + "Import/Lists/Specialists").subscribe(result => {
      this.specialists = result;
    }, error => console.log(error), () => this.requestProgress = false );
  }

  public upload(){
    if (this.fileToUpload && this.specialistId){
      let formData = new FormData();
      formData.append('import', this.fileToUpload, this.fileToUpload.name);
      this.requestProgress = true;
      this.http.post<ImportReport>(this.baseUrl + 'Import/Symptoms?specialistId=' + this.specialistId, formData).subscribe(result => {
        this.setupReportAndOptions(result);
      }, error => console.error(error), () => this.requestProgress = false );
    }
  }
}

class Specialist{
  public id: number;
  public name: string;
}
