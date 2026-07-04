import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidationMessages } from './validation-messages';

describe('ValidationMessages', () => {
  let component: ValidationMessages;
  let fixture: ComponentFixture<ValidationMessages>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ValidationMessages],
    }).compileComponents();

    fixture = TestBed.createComponent(ValidationMessages);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
