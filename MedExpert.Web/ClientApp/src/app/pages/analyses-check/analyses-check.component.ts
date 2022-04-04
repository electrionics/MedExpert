import { Component, OnInit } from '@angular/core';
import {FormBuilder, Validators} from '@angular/forms';
import {Select, Store} from '@ngxs/store';
import {GetSpecialistsAction} from '../../store/actions/analyses.actions';
import {Observable} from 'rxjs';
import {AnalysesState} from '../../store/state/analyses.state';
import {ISelectOptions} from '../../store/model/select-option.model';
import {IParameters} from '../../store/model/parameter.model';
import {tap} from 'rxjs/operators';

@Component({
  selector: 'app-analyses-check',
  templateUrl: './analyses-check.component.html',
  styleUrls: ['./analyses-check.component.css']
})
export class AnalysesCheckComponent implements OnInit {
  @Select(AnalysesState.GetGenders) genders$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetSpecialists) specialists$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetParameters) parameters$: Observable<IParameters>;

  public patientForm = this.formBuilder.group({
    gender: [null, Validators.required],
    age: [null, Validators.required],
    specialists: [{ value: null, disabled: true }]
  })

  public parametersForm = this.formBuilder.group({
    items: this.formBuilder.array([])
  })

  constructor(
    private readonly store: Store,
    private readonly formBuilder: FormBuilder
  ) { }

  public ngOnInit(): void {
    const { gender, age } = this.patientForm.value;

    this.store.dispatch(new GetSpecialistsAction(gender, age));

    this.parameters$
      .pipe(tap(({items}) => {
        this.parametersForm = this.formBuilder.group({
          items: this.formBuilder.array(items.map((item) => {
            return this.formBuilder.group({
              item: [item],
              from: [item.referenceValues[0]],
              to: [item.referenceValues[1]],
              result: [],
            })
          }))
        });

        console.log(this.parametersForm);
      }))
      .subscribe();
  }

  public save(): void {
    console.log('save')
  }
}
