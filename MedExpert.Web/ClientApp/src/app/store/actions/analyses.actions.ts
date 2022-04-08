import {IIndicator} from '../model/indicator.model';
import {ISelectOption} from '../model/select-option.model';

export class GetSpecialistsAction {
  static readonly type = '[GET] Specialists';

  constructor(public sex: string, public age: number) {}
}

export class GetIndicatorsAction {
  static readonly type = '[GET] Indicators';

  constructor() {}
}

export class GetResultsAction {
  static readonly type = '[GET] Results';

  constructor(public sex: string, public age: number, public indicators: IIndicator[], public specialists: ISelectOption[]) {}
}
