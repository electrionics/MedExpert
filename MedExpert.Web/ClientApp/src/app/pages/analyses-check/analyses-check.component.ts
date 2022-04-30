import {ChangeDetectorRef, Component, OnInit} from '@angular/core';
import {FormArray, FormBuilder, Validators} from '@angular/forms';
import {Select, Store} from '@ngxs/store';
import {GetIndicatorsAction, GetResultsAction, GetSpecialistsAction} from '../../store/actions/analyses.actions';
import {combineLatest, Observable} from 'rxjs';
import {AnalysesState} from '../../store/state/analyses.state';
import {ISelectOptions, SelectOptionsDTO} from '../../store/model/select-option.model';
import {IIndicators, IndicatorsDTO} from '../../store/model/indicator.model';
import {debounceTime, filter, switchMap, tap} from 'rxjs/operators';
import {conditionalValidator, FormsService, FormStateEnum} from '../../services/forms.service';

@Component({
  selector: 'app-analyses-check',
  templateUrl: './analyses-check.component.html',
  styleUrls: ['./analyses-check.component.css']
})
export class AnalysesCheckComponent implements OnInit {
  @Select(AnalysesState.GetSexes) public readonly sexes$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetSpecialists) public readonly specialists$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetIndicators) private readonly indicators$: Observable<IIndicators>;

  public patientFormState = FormStateEnum.pristine;
  public patientFormStateEnum = FormStateEnum;
  public indicatorsFormDirty = false;

  public readonly patientForm = this.formBuilder.group({
    sex: [null, { validators: Validators.required, updateOn: 'blur' }],
    age: [null, {
      validators: [Validators.required, Validators.min(0), Validators.max(100)],
      updateOn: 'blur'
    }]
  })

  public indicatorsForm = this.formBuilder.group({
    indicators: this.formBuilder.array([], {
      updateOn: 'change'
    }),
    specialists: []
  })

  constructor(
    private readonly store: Store,
    private readonly formBuilder: FormBuilder,
    private readonly formsService: FormsService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  public ngOnInit(): void {
    console.log(this);

    this.indicatorsForm.valueChanges
      .subscribe(() => {
        this.formsService.updateTreeValidity(this.indicatorsForm);
      });

    this.indicators$
      .pipe(tap(({items}) => {
        this.indicatorsForm.setControl('indicators', this.formBuilder.array(items.map(item => {
          return this.formBuilder.group({
            item: [item],
            min: [item.referenceIntervalMin, [
              conditionalValidator(formControl => formControl.value > formControl.parent.get('max').value, { outOfRange: true }),
              conditionalValidator(formControl => formControl.parent.get('result').value && !formControl.parent.get('min').value && !formControl.parent.get('max').value, { noRange: true })
            ]],
            // - - 4
            max: [item.referenceIntervalMax, [
              conditionalValidator(formControl => formControl.parent.get('min').value > formControl.value, { outOfRange: true }),
              conditionalValidator(formControl => formControl.parent.get('result').value && !formControl.parent.get('min').value && !formControl.parent.get('max').value, { noRange: true })
            ]],
            result: [{value: null, disabled: item.dependencyIndicatorIds != null}],
          })
        })));

        this.indicatorsForm.markAllAsTouched();
      }))
      .subscribe();

    combineLatest([
      this.patientForm.get('sex').valueChanges,
      this.patientForm.get('age').valueChanges.pipe(debounceTime(300))
    ])
      .pipe(
        filter(([ sex, age ]) => this.patientForm.valid && !!sex && !!age),
        switchMap(([ sex, age ]) => this.store.dispatch(new GetSpecialistsAction(sex.id, age)))
      )
      .subscribe();
  }

  public savePatientForm(): void {
    const { sex: { id: sexId }, age } = this.patientForm.value;

    this.store.dispatch(new GetIndicatorsAction(sexId, age))
      .subscribe(() => {
        this.patientFormState = FormStateEnum.saved;
        this.patientForm.disable();
      });
  }

  public resavePatientForm(): void {
    const { sex: { id: sexId }, age } = this.patientForm.value;

    this.store.dispatch(new GetIndicatorsAction(sexId, age))
      .subscribe(() => {
        this.patientFormState = FormStateEnum.saved;
        this.patientForm.disable();
      });
  }

  public changePatientForm(): void {
    this.patientFormState = FormStateEnum.changing;
    this.patientForm.enable();
  }

  public getResults(): void {
    this.indicatorsFormDirty = true;

    if (this.indicatorsForm.invalid) {
      return;
    }

    const { sex: { id: sexId }, age } = this.patientForm.value;

    const indicators = new IndicatorsDTO().fromForm(this.indicatorsForm.get('indicators'));
    const specialists = new SelectOptionsDTO().fromForm(this.indicatorsForm.get('specialists'));

    this.store.dispatch(new GetResultsAction(sexId, age, indicators.items, specialists.items));
  }

  get showSaveButton() {
    return this.patientFormState & this.patientFormStateEnum.pristine;
  }

  get showChangeButton() {
    return this.patientFormState & this.patientFormStateEnum.saved;
  }

  get showResaveButton() {
    return this.patientFormState & this.patientFormStateEnum.changing;
  }

  public hasError(name: string): boolean {
    return this.formsService.hasError(this.patientForm, name);
  }
}
