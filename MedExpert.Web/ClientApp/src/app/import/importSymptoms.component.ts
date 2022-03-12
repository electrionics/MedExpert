import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {ImportBaseComponent, ImportReport} from "./importBase";
import {FormControl, Validators} from "@angular/forms";

@Component({
  selector: 'app-import-symptoms',
  templateUrl: './importSymptoms.component.html'
})
export class ImportSymptomsComponent extends ImportBaseComponent{
  private readonly http: HttpClient;
  private readonly baseUrl: string

  public NewSpecialistName: FormControl;
  public SpecialistId: FormControl;

  public isNewSpecialist: boolean;

  public specialists: Lookup[];
  public categories: Lookup[];
  public sex: Lookup[]

  public specialist: SpecialistImport;

  private requestCounter: number;

  constructor(http: HttpClient, @Inject('BASE_API_URL') baseUrl: string) {
    super();

    this.http = http;
    this.baseUrl = baseUrl;
    this.requestCounter = 0;
  }

  ngOnInit(){
    this.loadLookups();

    this.specialist = new SpecialistImport();
    this.specialist.SymptomCategoryId = 1;
    this.specialist.SpecialistId = null;
    this.specialist.NewSpecialistName = null;
    this.specialist.NewSpecialistSex = 0;

    const nameRegexp = '^[a-zA-Zа-яА-Я0-9-,() ]*$';
    this.NewSpecialistName = new FormControl(this.specialist.NewSpecialistName, [Validators.required, Validators.maxLength(100), Validators.pattern(nameRegexp)]);
    this.SpecialistId = new FormControl(this.specialist.SpecialistId, [Validators.required]);
  }

  public upload() {
    if (this.isFormValid() && !this.requestProgress) {
      let formData = new FormData();
      formData.append('import', this.fileToUpload, this.fileToUpload.name);

      formData.append('SymptomCategoryId', this.specialist.SymptomCategoryId?.toString());

      if (this.isNewSpecialist){
        formData.append('NewSpecialistName', this.specialist.NewSpecialistName);
        if (this.specialist.NewSpecialistSex){
          formData.append('NewSpecialistSex', this.specialist.NewSpecialistSex.toString());
        }
      }
      else {
        formData.append('SpecialistId', this.specialist.SpecialistId?.toString());
      }

      this.requestProgress = true;
      this.requestCounter = 1;
      this.http.post<ImportReport>(this.baseUrl + 'Import/Symptoms', formData).subscribe(result => {
        this.setupReportAndOptions(result);

        if (!this.Report.countInvalidRows &&
            !this.Report.totalInsertedErrorsCount &&
            !this.Report.totalUpdatedErrorsCount &&
            !this.Report.error &&
            this.Report.headerValid){
          this.specialist.NewSpecialistName = null;
          this.specialist.NewSpecialistSex = 0;
        }

        if (this.Report.result){
          this.specialist.SpecialistId = Number.parseInt(this.Report.result);
          this.isNewSpecialist = false;
        }

        this.updateControls();

        this.loadLookups();
      }, error => console.error(error), () => {
        this.requestCounter--;
        if (this.requestCounter == 0) {
          this.requestProgress = false
        }
      });
    }
  }

  public isFormValid(){
    return this.fileToUpload && this.specialist.SymptomCategoryId && (this.isNewSpecialist
      ? !this.NewSpecialistName.invalid
      : !this.SpecialistId.invalid);
  }

  public updateControls(){
    this.NewSpecialistName.setValue(this.specialist.NewSpecialistName);
    this.SpecialistId.setValue(this.specialist.SpecialistId);
    this.NewSpecialistName.updateValueAndValidity();
    this.SpecialistId.updateValueAndValidity();
  }

  private loadLookups(){
    this.requestProgress = true;
    this.requestCounter++;
    this.http.get<{[name: string]: Lookup[]}>(this.baseUrl + "Import/Lists/Lookups").subscribe(result => {
      this.specialists = result["Specialists"];
      this.categories = result["Categories"];
      this.sex = result["Sex"];
    }, error => console.log(error), () => {
      this.requestCounter--;
      if (this.requestCounter == 0) {
        this.requestProgress = false
      }
    });
  }
}

class Lookup{
  public id: number;
  public name: string;
}

class SpecialistImport{
  public SpecialistId: number;
  public SymptomCategoryId: number;
  public NewSpecialistName: string;
  public NewSpecialistSex: number;
}
