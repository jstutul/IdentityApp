import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Account } from '../../services/account';
import { Shared } from '../../services/shared';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';
import { RegisterWithExternal } from '../../shared/models/account/registerWithExternal';

@Component({
  selector: 'app-register-with-third-party',
  imports: [ReactiveFormsModule, ValidationMessages],
  templateUrl: './register-with-third-party.html',
  styleUrl: './register-with-third-party.css',
})
export class RegisterWithThirdParty {
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  provider:string|null=null;
  acccess_token:string|null=null;
  userId:string|null=null;
  errorMessages:string[] = [];
  private accountService = inject(Account);
  private sharedService = inject(Shared);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private activeRoute = inject(ActivatedRoute);
  constructor(private formBuilder: FormBuilder){}
  ngOnInit(){
    this.accountService.user$.pipe(take(1)).subscribe({
        next:(user:User|null)=>{
          if(user){
            this.router.navigateByUrl('/');
          }else{
            this.activeRoute.queryParamMap.subscribe({
              next:(params:any)=>{
                this.provider = this.activeRoute.snapshot.paramMap.get('provider');
                this.acccess_token =params.get('access_token');
                this.userId = params.get('userId');
                if(this.provider && this.acccess_token && this.userId && 
                  (this.provider === 'facebook' || this.provider ==='google')){
                    this.initializeForm();
                }else{
                  this.router.navigateByUrl('accounts/register');
                }
              }
            })
          }
        }
      })
  }
  initializeForm() {
    this.registerForm = this.formBuilder.group({
      firstName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]),
      lastName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]),
    });
  }

  register(){
    this.submitted = true;
    this.errorMessages = [];
    if(this.registerForm.valid && this.userId && this.provider && this.acccess_token){
      const firstName = this.registerForm.get('firstName')?.value;
      const lastName = this.registerForm.get('firstName')?.value;
      const model = new RegisterWithExternal(firstName,lastName,this.userId,this.acccess_token,this.provider);
      console.log(model);
      this.accountService.registerWithThirdParty(model).subscribe({
        next: _ => {
          this.router.navigateByUrl('/');
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
    }
    
  }
}
