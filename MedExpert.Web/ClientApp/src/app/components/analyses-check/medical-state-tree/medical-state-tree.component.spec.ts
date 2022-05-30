import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MedicalStateTreeComponent } from './medical-state-tree.component';

describe('MedicalStateTreeComponent', () => {
  let component: MedicalStateTreeComponent;
  let fixture: ComponentFixture<MedicalStateTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MedicalStateTreeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MedicalStateTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
