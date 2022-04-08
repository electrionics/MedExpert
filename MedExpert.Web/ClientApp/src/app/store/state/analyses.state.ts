import { Injectable } from '@angular/core';
import {Action, Selector, State, StateContext} from '@ngxs/store';
import {GetIndicatorsAction, GetResultsAction, GetSpecialistsAction} from '../actions/analyses.actions';
import {AnalysesService} from '../../services/analyses.service';
import {tap} from 'rxjs/operators';
import {ISelectOptions, SelectOptionsDTO} from '../model/select-option.model';
import {IIndicators, IndicatorsDTO} from '../model/indicator.model';

export interface IAnalysesState {
  sexes: ISelectOptions;
  specialists: ISelectOptions;
  indicators: IIndicators;
}

@State<IAnalysesState>({
  name: 'analyses',
  defaults: {
    sexes: new SelectOptionsDTO([
      { id: 1, name: "Мужчина" },
      { id: 2, name: "Женщина" }
    ]),
    specialists: new SelectOptionsDTO([]),
    indicators: new IndicatorsDTO([]),
  }
})
@Injectable()
export class AnalysesState {
  constructor(
    private readonly analysesService: AnalysesService
  ) { }

  @Selector()
  static GetSexes({ sexes }: IAnalysesState) {
    return sexes;
  }

  @Selector()
  static GetSpecialists({ specialists }: IAnalysesState) {
    return specialists;
  }

  @Selector()
  static GetIndicators({ indicators }: IAnalysesState) {
    return indicators;
  }

  @Action(GetSpecialistsAction)
  GetSpecialistsAction({ patchState }: StateContext<IAnalysesState>, { sex, age }: GetSpecialistsAction) {
    return this.analysesService.getSpecialists(sex, age).pipe(tap((specialists) => {
      patchState({ specialists: new SelectOptionsDTO(specialists) });
    }))
  }

  @Action(GetIndicatorsAction)
  GetIndicatorsAction({ patchState }: StateContext<IAnalysesState>, { sex, age }: GetIndicatorsAction) {
    return this.analysesService.getIndicators(sex, age).pipe(tap((indicators) => {
      patchState({ indicators: new IndicatorsDTO(indicators) });
    }))
  }

  @Action(GetResultsAction)
  GetResultsAction({ patchState }: StateContext<IAnalysesState>, body: GetResultsAction) {
    return this.analysesService.getResults(body).pipe(tap((response) => {
      console.log(response)
    }))
  }
}
