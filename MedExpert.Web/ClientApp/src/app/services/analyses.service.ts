import { Injectable } from '@angular/core';
import {ApiService} from './api.service';
import {IIndicator} from '../store/model/indicator.model';
import {ISelectOption} from '../store/model/select-option.model';

@Injectable({
  providedIn: 'root'
})
export class AnalysesService {
  constructor(
    private readonly apiService: ApiService
  ) { }

  public getSpecialists(sex: string, age: number) {
    return this.apiService.post<ISelectOption[]>('Analysis/Specialists', {
      sex,
      age
    })
  }

  public getIndicators(sex: string, age: number) {
    return this.apiService.post<IIndicator[]>('Analysis/Indicators', {
      sex,
      age
    })
  }

  public saveAnalysisResult(body) {
    return this.apiService.post('Analysis/Calculate', body);
  }

  public getAnalysisResultById(body) {
    return this.apiService.post('Analysis/FilterResults', body);
  }

  public getComputedIndicators({indicatorValues}) {
    return this.apiService.post('Analysis/ComputeIndicators', indicatorValues)
  }
}
