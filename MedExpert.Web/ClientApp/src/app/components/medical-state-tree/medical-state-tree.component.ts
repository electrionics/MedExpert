import {Component, Input, OnInit} from '@angular/core';
import {IMedicalState} from "../../store/model/medical-state.model";
import {TreeItem} from "../../store/model/tree-item";

@Component({
  selector: 'app-medical-state-tree',
  templateUrl: './medical-state-tree.component.html',
  styleUrls: ['./medical-state-tree.component.css'],
})

export class MedicalStateTreeComponent implements OnInit {
  public recommendedAnalysesListShown: boolean;

  @Input()
  medicalState: TreeItem<IMedicalState>;

  @Input()
  isOpen?: boolean;

  @Input()
  specialistsMap: Map<number, string>;

  @Input()
  nestingLevel?: number = 0;


  get hasChildren():boolean {
    return this.medicalState.children && this.medicalState.children.length != 0;
  }
  constructor() {
    this.isOpen = true;
  }

  ngOnInit(): void {
  }

  public openChildren() {
    this.isOpen = !this.isOpen;
  }

  public toggleRecommendedAnalysesList() {
    if (this.medicalState.item.recommendedAnalyses.length > 0) {
      this.recommendedAnalysesListShown = !this.recommendedAnalysesListShown;
    }
  }

}
