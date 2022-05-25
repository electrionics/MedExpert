import {IIndicator} from "./indicator.model";

export interface IMedicalState {
  symptomId: number;
  name: string;
  severity: number;
  combinedSubtreeSeverity: number;
  recommendedAnalyses: IIndicator[];
  specialistId: number;
}
