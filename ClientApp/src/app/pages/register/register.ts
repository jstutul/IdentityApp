import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from '../../services/account';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, ValidationMessages],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages:string[] = [];
  private accountService = inject(Account);
   private cdr = inject(ChangeDetectorRef);
  constructor(private formBuilder: FormBuilder) {
  }
  ngOnInit() {
    this.initializeForm();
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      firstName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]),
      lastName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]),
      email: new FormControl('', [Validators.required,  Validators.email,Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)]),
      password: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(15)]),
    });
  }

  register(){
    this.submitted = true;
    this.errorMessages = [];
    // if(this.registerForm.valid){
      this.accountService.register(this.registerForm.value).subscribe({
        next: (response) => {
          console.log('Registration successful:', response);  
        },
        error: (error) => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
            console.log('Error messages:', this.errorMessages);
            this.registerForm.markAllAsTouched();
          }else{
            this.errorMessages.push(error.error);
          }
          this.cdr.detectChanges();
        }
      });
    // }
    
  }
}
