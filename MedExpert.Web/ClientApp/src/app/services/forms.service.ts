import { Injectable } from '@angular/core';
import {FormArray, FormControl, FormGroup, ValidationErrors, ValidatorFn} from '@angular/forms';

export enum FormStateEnum {
  pristine = 1,
  saved = 2,
  changing = 4
}

export function conditionalValidator(
  predicate: Predicate,
  error: {[key: string]: boolean}
) {
  return (formControl: FormControl): ValidationErrors => {
    if (!formControl.parent) {
      return null
    }

    if (predicate(formControl)) {
      return error
    }

    return null
  }
}

type Predicate = (formControl: FormControl) => boolean

@Injectable({
  providedIn: 'root'
})
export class FormsService {
  public hasError(formGroup: FormGroup, name: string): boolean {
    return formGroup.get(name).invalid && (formGroup.get(name).dirty || formGroup.get(name).touched);
  }

  public updateTreeValidity(group: FormGroup | FormArray): void {
    Object.keys(group.controls).forEach((key: string) => {
      const abstractControl = group.controls[key];

      if (abstractControl instanceof FormGroup || abstractControl instanceof FormArray) {
        this.updateTreeValidity(abstractControl);
      } else {
        abstractControl.updateValueAndValidity({ emitEvent: false });
      }
    });
  }
}
