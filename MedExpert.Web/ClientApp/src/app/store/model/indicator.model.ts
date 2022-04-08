import {AbstractControl} from '@angular/forms';

export interface IIndicator {
  id: number;
  name: string;
  referenceIntervalMax: number;
  referenceIntervalMin: number;
  shortName: string
  value: number;
}

export interface IIndicators {
  items: IIndicator[];
}

export class IndicatorsDTO implements IIndicators {
  constructor(
    public items: IIndicator[] = []
  ) { }

  public fromForm(group: AbstractControl): IIndicators {
    this.items = group.value.map(item => ({
      ...item.item,
      referenceIntervalMin: item.min,
      referenceIntervalMax: item.max,
      value: item.result
    }));

    return this;
  }
}
