import {AbstractControl} from '@angular/forms';

export interface ISelectOption {
  id: number;
  name: string;
}

export interface ISelectOptions {
  items: ISelectOption[];
}

export class SelectOptionsDTO implements ISelectOptions {
  constructor(
    public items: ISelectOption[] = []
  ) { }

  public fromForm(group: AbstractControl): ISelectOptions {
    this.items = group.value;

    return this;
  }
}
