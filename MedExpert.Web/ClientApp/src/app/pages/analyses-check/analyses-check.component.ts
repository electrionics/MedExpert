import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnInit,
  QueryList,
  Renderer2,
  ViewChild,
  ViewChildren
} from '@angular/core';
import {FormArray, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Select, Store} from '@ngxs/store';
import {
  GetAnalysisResultByIdAction,
  GetComputedIndicatorsAction,
  GetIndicatorsAction,
  GetSpecialistsAction,
  SaveAnalysisResultAction
} from '../../store/actions/analyses.actions';
import {combineLatest, Observable} from 'rxjs';
import {AnalysesState} from '../../store/state/analyses.state';
import {ISelectOption, ISelectOptions, SelectOptionsDTO} from '../../store/model/select-option.model';
import {IIndicator, IIndicators, IndicatorsDTO} from '../../store/model/indicator.model';
import {debounceTime, filter, switchMap, tap} from 'rxjs/operators';
import {conditionalValidator, FormsService, FormStateEnum} from '../../services/forms.service';
import {ProfileDTO} from "../../store/model/profile.model";
import {IComputedIndicator} from "../../store/model/computed-indicator.model";
import {IAnalysesResult, MedicalStateFilterType} from "../../store/model/analyses-result.model";
import {IFilterButton} from "../../store/model/filter-button.model";
import {MedicalStateTreeComponent} from "../../components/analyses-check/medical-state-tree/medical-state-tree.component";

@Component({
  selector: 'app-analyses-check',
  templateUrl: './analyses-check.component.html',
  styleUrls: ['./analyses-check.component.css']
})
export class AnalysesCheckComponent implements OnInit {
  @ViewChildren(MedicalStateTreeComponent) medicalStateTreeComponents!: QueryList<MedicalStateTreeComponent>;
  @ViewChild('commentsSection') commentsSection: ElementRef<HTMLElement>;
  // using ViewChildren here not because there are multiple 'resultsSection' (there is only one of course),
  // but because we need resultsSection.changes property (in order to scroll to element when it's shown)
  // and ViewChild doesn't seem to have this property
  @ViewChildren('resultsSection') resultsSection: QueryList<ElementRef<HTMLElement>>;

  private analysisId: number;
  private unsubscribeFromWindowClick: () => void;

  @Select(AnalysesState.GetSexes) public readonly sexes$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetSpecialists) public readonly specialists$: Observable<ISelectOptions>;
  @Select(AnalysesState.GetIndicators) private readonly indicators$: Observable<IIndicators>;
  @Select(AnalysesState.GetComputedIndicators) private readonly computedIndicators$: Observable<IComputedIndicator[]>;
  @Select(AnalysesState.GetAnalysisId) private readonly analysisId$: Observable<number>;
  @Select(AnalysesState.GetAnalysisResult) private readonly analysisResult$: Observable<IAnalysesResult>;

  public patientFormState = FormStateEnum.pristine;
  public patientFormStateEnum = FormStateEnum;
  public indicatorsFormDirty = false;
  public readonly filterButtons: IFilterButton[];
  public isAnalysisResultReceived = false;
  public allSpecialistsList: ISelectOption[];
  public allSpecialistsMap: Map<number, string> = new Map<number, string>();
  public analysesResult: IAnalysesResult;
  public loadingAnalysisResult: boolean;

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
    specialistsForCalculation: [null, {
      validators: [Validators.required],
      updateOn: 'change',
    }],
  })

  public analysesResultFiltersForm = this.formBuilder.group({
    specialistsForDisplay: [this.specialistsForCalculation, {
      validators: [Validators.required],
      updateOn: 'change',
    }]
  });

  get specialistsForCalculation() : ISelectOption[] {
    return new SelectOptionsDTO().fromForm(this.indicatorsForm.get('specialistsForCalculation')).items;
  }

  get specialistsForDisplay() : ISelectOption[] {
    return new SelectOptionsDTO().fromForm(this.analysesResultFiltersForm.get('specialistsForDisplay')).items;
  }

  get indicators(): IIndicator[] {
    return new IndicatorsDTO().fromForm(this.indicatorsForm.get('indicators')).items;
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

  get selectedFilterButton(): IFilterButton {
    return this.filterButtons.find(filterButton => filterButton.isSelected);
  }

  constructor(
    private readonly store: Store,
    private readonly formBuilder: FormBuilder,
    private readonly formsService: FormsService,
    private readonly cdr: ChangeDetectorRef,
    private readonly renderer: Renderer2,
  ) {
    this.filterButtons = [
      {
        value: MedicalStateFilterType.Diseases,
        name: 'Болезни',
        isSelected: true,
        titleForResults: 'Диагноз',
      },
      {
        value: MedicalStateFilterType.CommonAnalysis,
        name: 'Общие исследования',
        titleForResults: 'Исследование',
      },
      {
        value: MedicalStateFilterType.SpecialAnalysis,
        name: 'Специальные исследования',
        titleForResults: 'Исследование',
      },
      {
        value: MedicalStateFilterType.CommonTreatment,
        name: 'Общее лечение',
        titleForResults: 'Лечение',
      },
      {
        value: MedicalStateFilterType.SpecialTreatment,
        name: 'Специальное лечение',
        titleForResults: 'Лечение',
      },
    ];
  }

  public ngOnInit(): void {
    this.indicatorsForm.valueChanges
      .subscribe(() => {
        this.formsService.updateTreeValidity(this.indicatorsForm);
      });

    this.indicators$
      .pipe(tap(({items}) => {
        if (!items || !items.length) return;
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

        this.indicatorsForm.get('indicators').markAllAsTouched();
      }))
      .subscribe();

    this.computedIndicators$.subscribe(computedIndicators => {
      if (!computedIndicators || !computedIndicators.length) return;
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
      this.analysisId = analysisId;
      this.getAnalysisResult(this.specialistsForCalculation);
    });

    this.analysisResult$.subscribe(analysesResult => {
      if (!analysesResult) return;
      this.isAnalysisResultReceived = true;
      this.analysesResult = analysesResult;
      this.loadingAnalysisResult = false;
      // TODO: find better solution for setting default value for "Specialists for display" select
      // maybe in this.resultsSection.changes.subscribe ?

      // check if Specialists for display value is not defined - that means it was not set manually yet
      if (!this.specialistsForDisplay || !this.specialistsForDisplay.length) {
        // set default value of "Specialists for display" select when it's rendered
        // problem: without setTimeout Analysis Result Filter form is not rendered yet.
        //  And when we select all specialists for "Specialists for display" select it throws and error "cannot set property of undefined"
        setTimeout(() => {
          this.selectAllSpecialistsForDisplay();
        }, 0);
      }
    });

    this.specialists$.subscribe(specialists => {
      this.allSpecialistsList = specialists.items;
      this.allSpecialistsList.forEach( specialist => {
        this.allSpecialistsMap.set(specialist.id, specialist.name);
      });
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

    // we subscribe to window click not in MedicalStateTreeComponent, but here - because we want to subscribe to it only in one place, not in every tree node
    this.unsubscribeFromWindowClick = this.renderer.listen('window', 'click', (event) => {
      if (this.isAnalysisResultReceived && this.medicalStateTreeComponents) {
        this.medicalStateTreeComponents.toArray().forEach((medicalStateTreeComponent) => {
          medicalStateTreeComponent.onWindowClick(event);
        })
      }
    });
  }

  public ngAfterViewInit(): void
  {
    this.resultsSection.changes.subscribe((sectionQueryList: QueryList<ElementRef<HTMLElement>>) =>
    {
      // scroll to Results section when it's first rendered
      this.scrollTo(sectionQueryList.first);
    });
  }

  ngOnDestroy() {
    if (this.unsubscribeFromWindowClick) {
      this.unsubscribeFromWindowClick();
    }
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

    const specialistIds = this.specialistsForCalculation.map(x => x.id);
    const profile = new ProfileDTO().fromForm(this.patientForm);

    // send request to retrieve analysis id
    this.store.dispatch(new SaveAnalysisResultAction(profile, this.indicators, specialistIds))
      .subscribe(() => {
        this.indicatorsForm.disable();
      });
  }

  public getAnalysisResult(specialists: ISelectOption[]) {
    this.loadingAnalysisResult = true;
    const specialistIds = specialists.map(x => x.id);
    this.store.dispatch(new GetAnalysisResultByIdAction(this.analysisId, this.selectedFilterButton.value, specialistIds));
  }

  public getComputedIndicators(): void {
    // generate proper body for ComputeIndicators request
    const indicatorValues: IComputedIndicator[] = this.indicators.map(indicator => {
      return {id: indicator.id, value: indicator.value}
    });
    // send the request to server
    this.store.dispatch(new GetComputedIndicatorsAction(indicatorValues))
      .subscribe();
  }

  public hasError(name: string): boolean {
    return this.formsService.hasError(this.patientForm, name);
  }

  public indicatorValueChanged(changedIndicatorId: number, changedIndicatorValue: number) {
    const indicators = this.indicators;
    // check if changed indicator is a dependency for any of calculated indicators
    const isDependency = indicators.some(indicator => {
      return indicator.dependencyIndicatorIds && indicator.dependencyIndicatorIds.includes(changedIndicatorId);
    });
    // only if changed indicator is a dependency, send request to server to calculate computed indicators
    if (isDependency) {
      this.getComputedIndicators();
    }
  }

  public selectFilter(selectedButton: IFilterButton) {
    this.filterButtons.forEach(filterButton => {
      filterButton.isSelected = false;
    });
    selectedButton.isSelected = true;
    this.getAnalysisResult(this.specialistsForDisplay);
  }

  public selectAllSpecialistsForCalculation() {
    this.indicatorsForm.patchValue({
      specialistsForCalculation: this.allSpecialistsList,
    });
  }

  public selectAllSpecialistsForDisplay() {
    this.analysesResultFiltersForm.patchValue({
      specialistsForDisplay: this.specialistsForCalculation,
    });
  }

  // Easter egg for easier testing
  // Populates values of all unset indicators, their mins and maxes.
  // Activates on Shift + click on "Показатели" title
  public fillIndicatorsWithTestData(event: MouseEvent) {

    if (event.shiftKey) {
      const indicatorsForm = this.indicatorsForm.get('indicators') as FormArray;
      const indicatorFormControls = indicatorsForm.controls;
      for (let i = 1; i < indicatorFormControls.length + 1; i++) {
        let indicatorFormGroup = indicatorFormControls[i - 1] as FormGroup;
        let isComputed = indicatorFormGroup.controls.item.value.dependencyIndicatorIds && indicatorFormGroup.controls.item.value.dependencyIndicatorIds.length;
        // populate result value only if it's not a computed indicator
        if (!isComputed) {
          indicatorFormGroup.controls.result.setValue(i);
        }
        const minValue = indicatorFormGroup.controls.min.value;
        const maxValue = indicatorFormGroup.controls.max.value;
        // populate minValue if it's not set
        if (!minValue && minValue != 0) {
          indicatorFormGroup.controls.min.setValue(i * 2);
        }
        // populate maxValue if it's not set
        if (!maxValue && maxValue != 0) {
          indicatorFormGroup.controls.max.setValue(i * 3);
        }
      }
      indicatorsForm.markAllAsTouched();
      // calculate indicators
      this.getComputedIndicators();
      this.selectAllSpecialistsForCalculation();
    }
  }

  public filterResults() {
    const specialistsForDisplay = this.specialistsForDisplay;
    if (specialistsForDisplay.length !== 0) {
      this.getAnalysisResult(specialistsForDisplay);
    }
  }

  public openAllMedicalStates() {
    this.medicalStateTreeComponents.toArray().forEach(medicalStateTreeComponent => {
      medicalStateTreeComponent.openChildrenDeep()
    });
  }

  public closeAllMedicalStates() {
    this.medicalStateTreeComponents.toArray().forEach(medicalStateTreeComponent => {
      medicalStateTreeComponent.closeChildrenDeep();
    });
  }

  public scrollTo(element: ElementRef<HTMLElement>) {
    if (element) {
      element.nativeElement.scrollIntoView({behavior: "smooth"});
    }
  }
}
