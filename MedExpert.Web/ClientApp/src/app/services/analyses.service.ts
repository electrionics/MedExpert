import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AnalysesService {
  constructor(
    private readonly httpClient: HttpClient
  ) { }

  getSpecialists(gender: string, age: number) {
    return this.httpClient.get('/api/request', {
      params: {
        gender,
        age
      }
    })
  }
}
