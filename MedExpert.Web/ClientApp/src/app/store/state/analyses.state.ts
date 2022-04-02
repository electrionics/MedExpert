import { Injectable } from '@angular/core';
import {Action, Selector, State, StateContext} from '@ngxs/store';
import {GetSpecialistsAction} from '../actions/analyses.actions';
import {AnalysesService} from '../../services/analyses.service';
import {catchError, tap} from 'rxjs/operators';
import {ISelectOption, ISelectOptions, SelectOptionsDTO} from '../model/select-option.model';
import {IParameters, ParametersDTO} from '../model/parameter.model';

export interface IAnalysesState {
  genders: ISelectOptions;
  specialists: ISelectOptions;
  parameters: IParameters;
}

@State<IAnalysesState>({
  name: 'analyses',
  defaults: {
    genders: new SelectOptionsDTO([
      { id: '1', label: "Мужчина" },
      { id: '2', label: "Женщина" }
    ]),
    specialists: new SelectOptionsDTO([]),
    parameters: new ParametersDTO([
      { id: '1', label: "Параметр 1", referenceValues: [1, 2] },
      { id: '2', label: "Параметр 2", referenceValues: [4, 8] },
      { id: '3', label: "Параметр 3", referenceValues: [2, 5] },
    ]),
  }
})
@Injectable()
export class AnalysesState {
  constructor(
    private readonly analysesService: AnalysesService
  ) { }

  @Selector()
  static GetGenders({ genders }: IAnalysesState) {
    return genders;
  }

  @Selector()
  static GetSpecialists({ specialists }: IAnalysesState) {
    return specialists;
  }

  @Selector()
  static GetParameters({ parameters }: IAnalysesState) {
    return parameters;
  }

  @Action(GetSpecialistsAction)
  GetSpecialistsAction({ patchState }: StateContext<IAnalysesState>, { gender, age }: GetSpecialistsAction) {
    const specialists: ISelectOption[] = [
      {id: '1', label: 'специалист 1'},
      {id: '2', label: 'специалист 2'},
      {id: '3', label: 'специалист 3'}
    ];

    return this.analysesService.getSpecialists(gender, age).pipe(tap((_specialists) => {
      patchState({ specialists: new SelectOptionsDTO(specialists) });
    }), catchError(error => {
      patchState({ specialists: new SelectOptionsDTO(specialists) });

      throw error;
    }))
  }
}
