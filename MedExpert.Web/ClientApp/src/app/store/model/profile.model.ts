import {AbstractControl} from "@angular/forms"

export interface IProfile {
  sex: number;
  age: number;
}

export class ProfileDTO implements IProfile {
  constructor(
    public sex: number = null, public age: number = null
  ) { }

  public fromForm(group: AbstractControl): IProfile {
    const { sex: { id: sexVal }, age: ageVal } = group.value;

    this.sex = sexVal;
    this.age = ageVal;

    return this;
  }
}
