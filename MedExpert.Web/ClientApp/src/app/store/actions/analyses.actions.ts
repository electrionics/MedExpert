export class GetSpecialistsAction {
  static readonly type = '[GET] Specilists';

  constructor(public gender: string, public age: number) {}
}
