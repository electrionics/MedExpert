import {Component, ElementRef, Input, OnInit, QueryList, Renderer2, ViewChild, ViewChildren} from '@angular/core';
import {IMedicalState} from "../../../store/model/medical-state.model";
import {TreeItem} from "../../../store/model/tree-item";

@Component({
  selector: 'app-medical-state-tree',
  templateUrl: './medical-state-tree.component.html',
  styleUrls: ['./medical-state-tree.component.css'],
})

export class MedicalStateTreeComponent implements OnInit {
  @ViewChild('toggleContextualPopupButton') toggleContextualPopupButton: ElementRef;
  @ViewChild('contextualPopup') contextualPopup: ElementRef;
  @ViewChildren(MedicalStateTreeComponent) medicalStateTreeComponents!: QueryList<MedicalStateTreeComponent>;

  public recommendedAnalysesListShown: boolean;

  @Input()
  medicalState: TreeItem<IMedicalState>;

  @Input()
  isOpen?: boolean;

  @Input()
  specialistsMap: Map<number, string>;

  @Input()
  nestingLevel?: number = 0;


  get hasChildren(): boolean {
    return this.medicalState.children && this.medicalState.children.length != 0;
  }

  get hasSeverity(): boolean {
    return this.medicalState.item.severity !== null && this.medicalState.item.severity !== undefined;
  }

  get hasCombinedSubtreeSeverity(): boolean {
    return this.medicalState.item.combinedSubtreeSeverity !== null && this.medicalState.item.combinedSubtreeSeverity !== undefined;
  }

  get hasRecommendedAnalyses(): boolean {
    return this.medicalState.item.recommendedAnalyses.length !== 0
  }

  constructor(private renderer: Renderer2) {
    this.isOpen = true;
  }

  ngOnInit(): void {
  }

  public toggleChildren() {
    this.isOpen = !this.isOpen;
  }

  public openChildrenDeep() {
    // open itself
    this.isOpen = true;
    // open all children
    if (this.medicalStateTreeComponents) {
      this.medicalStateTreeComponents.toArray().forEach(child => {
        child.openChildrenDeep();
      });
    }
  }

  public closeChildrenDeep() {
    // close itself
    this.isOpen = false;
    // close all children
    if (this.medicalStateTreeComponents) {
      this.medicalStateTreeComponents.toArray().forEach(child => {
        child.closeChildrenDeep();
      });
    }
  }

  public toggleRecommendedAnalysesList() {
    if (this.medicalState.item.recommendedAnalyses.length > 0) {
      this.recommendedAnalysesListShown = !this.recommendedAnalysesListShown;
    }
  }

  // method to track clicks outside of contextual menu with recommended analyses
  public onWindowClick(event: PointerEvent) {
    const isClickOutsideToggleButton = !this.toggleContextualPopupButton.nativeElement.contains(event.target);
    const isClickOutsidePopup = !this.contextualPopup.nativeElement.contains(event.target);
    // Only close contextual popup if the click was made outside of toggle button and contextual popup
    // we need to omit toggle button so that popup won't be closed right after it was opened with toggle button
    if (isClickOutsideToggleButton && isClickOutsidePopup) {
      this.recommendedAnalysesListShown = false;
      if (this.medicalStateTreeComponents) {
        // call same method for all child nodes
        this.medicalStateTreeComponents.toArray().forEach(component => {
          component.onWindowClick(event);
        });
      }
    }
  }


}
