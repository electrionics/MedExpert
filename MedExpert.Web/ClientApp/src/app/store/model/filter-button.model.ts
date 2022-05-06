import {MedicalStateFilterType} from "./analyses-result.model";

export interface IFilterButton {
  name: string;
  value: MedicalStateFilterType;
  isSelected?: boolean;
}
