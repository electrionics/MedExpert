export interface IParameter {
  id: string;
  label: string;
  referenceValues: [number, number];
}

export interface IParameters {
  items: IParameter[];
}

export class ParametersDTO implements IParameters {
  constructor(
    public items: IParameter[] = []
  ) { }
}
