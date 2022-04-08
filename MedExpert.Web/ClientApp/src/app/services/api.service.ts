import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(
    private readonly httpClient: HttpClient
  ) { }

  public get<T>(url: string, ...args: any[]): Observable<T> {
    return this.request<Observable<T>>('get', url, ...args);
  }

  public post<T>(url: string, ...args: any[]): Observable<T> {
    return this.request<Observable<T>>('post', url, ...args);
  }

  private request<T>(method: string, url: string, ...args: any[]): T {
    return this.httpClient[method]<T>(`${environment.apiUrl}${url}`, ...args);
  }
}
