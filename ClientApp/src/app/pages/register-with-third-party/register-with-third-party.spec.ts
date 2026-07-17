import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterWithThirdParty } from './register-with-third-party';

describe('RegisterWithThirdParty', () => {
  let component: RegisterWithThirdParty;
  let fixture: ComponentFixture<RegisterWithThirdParty>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterWithThirdParty],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterWithThirdParty);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
