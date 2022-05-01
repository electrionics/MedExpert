import {IIndicator} from '../model/indicator.model';
import {IProfile} from "../model/profile.model";

export class GetSpecialistsAction {
  static readonly type = '[GET] Specialists';

  constructor(public sex: string, public age: number) {}
}

export class GetIndicatorsAction {
  static readonly type = '[GET] Indicators';

  constructor(public sex: string, public age: number) {}
}

export class GetResultsAction {
  static readonly type = '[GET] Results';

  constructor(public profile: IProfile, public indicators: IIndicator[], public specialistIds: number[]) {}
}
