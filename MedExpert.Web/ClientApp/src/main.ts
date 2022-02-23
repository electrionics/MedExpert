import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseApiUrl() {
  return document.getElementsByTagName('base')[0].href + 'Api/'; //
}

export function parseErrors(result: Result){
  this.errors = {};
  for (let i = 0; i < result.Errors.length; i++){
    let error = result.Errors[i];
    this[error.Key]["errors"]["server"] = error.Value;
  }
}

export interface Result{
  Success: boolean;
  Errors: {
    Key: string,
    Value: string
  }[];
}

const providers = [
  { provide: 'BASE_API_URL', useFactory: getBaseApiUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic(providers).bootstrapModule(AppModule)
  .catch(err => console.log(err));
