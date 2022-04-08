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

  public getIndicators() {
    return this.apiService.get<IIndicator[]>('Analysis/Indicators')
  }

  public getResults(body) {
    return this.apiService.post('Analysis/Calculate', body);
  }
}
