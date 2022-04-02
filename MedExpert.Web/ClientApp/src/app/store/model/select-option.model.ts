export interface ISelectOption {
  id: string;
  label: string;
}

export interface ISelectOptions {
  items: ISelectOption[];
}

export class SelectOptionsDTO implements ISelectOptions {
  constructor(
    public items: ISelectOption[] = []
  ) { }
}
