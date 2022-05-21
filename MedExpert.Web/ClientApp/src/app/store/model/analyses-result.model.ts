import {TreeItem} from "./tree-item";
import {IMedicalState} from "./medical-state.model";
import {IComment} from "./comment.model";

export interface IAnalysesResult {
  analysisId: number;
  foundMedicalStates: TreeItem<IMedicalState>[]
  comments: IComment[];
}

export enum MedicalStateFilterType {
  Diseases = 1,
  CommonTreatment = 2,
  SpecialTreatment = 3,
  CommonAnalysis = 4,
  SpecialAnalysis = 5
}
