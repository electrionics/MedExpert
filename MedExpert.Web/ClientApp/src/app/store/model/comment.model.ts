export interface IComment {
  symptomId: number;
  specialistId: number
  name: string;
  type: CommentType;
  text: string;
}

export enum CommentType {
  Symptom = 1,
  MatchedIndicator = 2,
  RecommendedForAnalysisIndicator = 3
}
