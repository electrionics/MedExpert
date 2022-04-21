import {Component, Inject} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {ImportBaseComponent, ImportReport} from "./importBase";
import {ApiService} from "../../services/api.service";

@Component({
  selector: 'app-import-indicators',
  templateUrl: './importIndicators.component.html'
})
export class ImportIndicatorsComponent extends ImportBaseComponent{
  private readonly http: HttpClient;
  private readonly baseUrl: string;
  private readonly apiService: ApiService;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string, apiService: ApiService) {
    super();

    this.http = http;
    this.baseUrl = baseUrl;
    this.apiService = apiService;
  }

  public upload(){
    if (this.fileToUpload){
      let formData = new FormData();
      formData.append('import', this.fileToUpload, this.fileToUpload.name);
      this.requestProgress = true;
      this.apiService.post<ImportReport>('Import/Indicators', formData).subscribe(result => {
        this.setupReportAndOptions(result);
      }, error => console.error(error), () => this.requestProgress = false );
    }
  }
}
