import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResetPasword } from './reset-pasword';

describe('ResetPasword', () => {
  let component: ResetPasword;
  let fixture: ComponentFixture<ResetPasword>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResetPasword],
    }).compileComponents();

    fixture = TestBed.createComponent(ResetPasword);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
