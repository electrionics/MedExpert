import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {ApiService} from "./api.service";
import {Router} from "@angular/router";

@Injectable({providedIn: 'root'})
export class AuthService {

  token$: BehaviorSubject<string> = new BehaviorSubject<string>(null);

  constructor(private apiService: ApiService, private router: Router) { }

  login(model, processResult: (res: LoginResultModel) => void) {
    return this.apiService.post<LoginResultModel>('Account/Login', model)
      .subscribe(result => {
        if (result.success){
          this.token$.next(result.token);
        }
        processResult(result);
      }, error => console.error(error));
  }

  logout() {
    this.token$.next(null);
    if (this.hasGuard([
      'canActivate'
    ], 'AuthGuard')){
      this.router.navigateByUrl('/account/login?returnUrl=' + this.router.url);
    }
  }

  get isLogged$(): Observable<boolean> {
    return this.token$.pipe(
      map(value => !!value)
    )
  }

  private hasGuard(guardTypeArray: string[], guardName: string) {
    const currentRouteConfig = this.router.config.find(f => f.path === this.router.url.substr(1));
    let hasGuard = false;

    if (currentRouteConfig) {
      for (const guardType of guardTypeArray) {
        if (!hasGuard) {
          if (currentRouteConfig[guardType] ) {
            for (const guard of currentRouteConfig[guardType]) {
              if (guard.name === guardName) {
                hasGuard = true;

                break;
              }
            }
          }
        } else {
          break;
        }
      }
    }
    return hasGuard;
  }
}

export class LoginResultModel{
  success: boolean;
  token: string;
  errorMessage: string;
  redirectUrl: string;
}
