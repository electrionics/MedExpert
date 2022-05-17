import {Component, Input, OnInit} from '@angular/core';
import {IMedicalState} from "../../store/model/medical-state.model";
import {TreeItem} from "../../store/model/tree-item";

@Component({
  selector: 'app-medical-state-tree',
  templateUrl: './medical-state-tree.component.html',
  styleUrls: ['./medical-state-tree.component.css'],
})

export class MedicalStateTreeComponent implements OnInit {
  @Input()
  medicalState: TreeItem<IMedicalState>;

  @Input()
  isOpen?: boolean;


  get hasChildren():boolean {
    return this.medicalState.children && this.medicalState.children.length != 0;
  }
  constructor() {
    // TODO remove this line
    this.isOpen = true;
  }

  ngOnInit(): void {
  }

  public openChildren() {
    // TODO uncomment this
    // this.isOpen = !this.isOpen;
  }

}
