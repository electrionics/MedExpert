import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import {
  catchError,
  first,
  mergeMap,
  mergeMapTo,
  withLatestFrom
} from 'rxjs/operators';
import { iif } from 'rxjs';
import { Router } from '@angular/router';
import {AuthService} from "../services/auth.service";

@Injectable({ providedIn: 'root' })
export class AuthInteceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private router: Router) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return this.authService.isLogged$
      .pipe(
        first(),
        withLatestFrom(this.authService.token$),
        mergeMap(([isLogged, tk]) =>
          iif(
            () => isLogged,
            next.handle(
              req.clone({ setHeaders: { Authorization: `Bearer ${tk}` } })
            ),
            next.handle(req)
          )
        )
      )
      .pipe(
        catchError(err => {
          if (err instanceof HttpErrorResponse) {
            switch (err.status) {
              case 404:
                console.log('redirect to login');
                this.authService.logout();
                this.router.navigateByUrl('/account/login');
                break;
              default:
            }
          }
          return of(err);
        })
      );
  }
}
