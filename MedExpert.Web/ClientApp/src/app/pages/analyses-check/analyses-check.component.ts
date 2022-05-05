import {ChangeDetectorRef, Component, OnInit} from '@angular/core';
import {FormArray, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Select, Store} from '@ngxs/store';
import {
  SaveAnalysisResultAction,
  GetComputedIndicatorsAction,
  GetIndicatorsAction,
  GetAnalysisResultByIdAction,
  GetSpecialistsAction
} from '../../store/actions/analyses.actions';
import {combineLatest, Observable} from 'rxjs';
import {AnalysesState} from '../../store/state/analyses.state';
import {ISelectOptions, SelectOptionsDTO} from '../../store/model/select-option.model';
import {IIndicators, IndicatorsDTO} from '../../store/model/indicator.model';
import {debounceTime, filter, switchMap, tap} from 'rxjs/operators';
import {conditionalValidator, FormsService, FormStateEnum} from '../../services/forms.service';
import {ProfileDTO} from "../../store/model/profile.model";
import {IComputedIndicator} from "../../store/model/computed-indicator.model";
import {IAnalysesResult, MedicalStateFilterType} from "../../store/model/analyses-result.model";

@Component({
  selector: 'app-analyses-check',
  templateUrl: './analyses-check.component.html',
  styleUrls: ['./analyses-check.component.css']
})
export class AnalysesCheckComponent implements OnInit {
  @Select(AnalysesState.GetSexes) public readonly sexes$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetSpecialists) public readonly specialists$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetIndicators) private readonly indicators$: Observable<IIndicators>;
  @Select(AnalysesState.GetComputedIndicators) private readonly computedIndicators$: Observable<IComputedIndicator[]>;
  @Select(AnalysesState.GetAnalysisId) private readonly analysisId$: Observable<number>;
  @Select(AnalysesState.GetAnalysisResult) private readonly analysisResult$: Observable<IAnalysesResult>;

  public patientFormState = FormStateEnum.pristine;
  public patientFormStateEnum = FormStateEnum;
  public indicatorsFormDirty = false;

  public readonly patientForm = this.formBuilder.group({
    sex: [null, { validators: Validators.required, updateOn: 'change' }],
    age: [null, {
      validators: [Validators.required, Validators.min(0), Validators.max(100)],
      updateOn: 'change'
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

    this.computedIndicators$.subscribe(computedIndicators => {
      const indicatorFormControls = (this.indicatorsForm.get('indicators') as FormArray).controls;
      computedIndicators.forEach(computedIndicator => {
        // find the form group where computed indicator input is located
        const computedIndicatorFormGroup = indicatorFormControls.find(formGroup => (formGroup as FormGroup).controls.item.value.id == computedIndicator.id) as FormGroup;
        // set computed indicator input value
        computedIndicatorFormGroup.controls.result.setValue(computedIndicator.value);
      });
    })

    this.analysisId$.subscribe((analysisId) => {
      if (!analysisId) return;
      this.getResultsByAnalysisId(analysisId);
    })

    this.analysisResult$.subscribe(analysesResult => {
      console.log('Analyses Result received', analysesResult);
      // TODO implement displaying results
    });

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

  public saveAnalysisResult(): void {
    this.indicatorsFormDirty = true;

    if (this.indicatorsForm.invalid) {
      return;
    }

    const indicators = new IndicatorsDTO().fromForm(this.indicatorsForm.get('indicators'));
    const specialistIds = new SelectOptionsDTO().fromForm(this.indicatorsForm.get('specialists')).items.map(x => x.id);
    const profile = new ProfileDTO().fromForm(this.patientForm);

    // send request to retrieve analysis id
    this.store.dispatch(new SaveAnalysisResultAction(profile, indicators.items, specialistIds));
  }

  public getResultsByAnalysisId(analysisId: number) {
    // TODO set current filter to MedicalStateFilterType.Diseases (that's the default value for filter type)
    const specialistIds = new SelectOptionsDTO().fromForm(this.indicatorsForm.get('specialists')).items.map(x => x.id);
    this.store.dispatch(new GetAnalysisResultByIdAction(analysisId, MedicalStateFilterType.Diseases, specialistIds));
  }

  public getComputedIndicators(): void {
    // generate proper body for ComputeIndicators request
    const indicators = new IndicatorsDTO().fromForm(this.indicatorsForm.get('indicators'));
    const indicatorValues: IComputedIndicator[] = indicators.items.map(indicator => {
      return {id: indicator.id, value: indicator.value}
    });
    // send the request to server
    this.store.dispatch(new GetComputedIndicatorsAction(indicatorValues))
      .subscribe();
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

  public indicatorValueChanged(changedIndicatorId: number, changedIndicatorValue: number) {
    const indicators = new IndicatorsDTO().fromForm(this.indicatorsForm.get('indicators'));
    // check if changed indicator is a dependency for any of calculated indicators
    const isDependency = indicators.items.some(indicator => {
      return indicator.dependencyIndicatorIds && indicator.dependencyIndicatorIds.includes(changedIndicatorId);
    });
    // only if changed indicator is a dependency, send request to server to calculate computed indicators
    if (isDependency) {
      this.getComputedIndicators();
    }
  }
}
