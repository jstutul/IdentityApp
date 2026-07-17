import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from '../../services/account';
import { Shared } from '../../services/shared';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, ValidationMessages, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages:string[] = [];
  returnUrl:string|null=null;
  private accountService = inject(Account);
  private sharedService = inject(Shared);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private activatedRoute = inject(ActivatedRoute);
  constructor(private formBuilder: FormBuilder) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next: (user:User |null)=>{
        console.log(user);
        if(user){
          this.router.navigateByUrl('/');
        }else{
          this.activatedRoute.queryParamMap.subscribe({
            next:(params:any)=>{
              this.returnUrl=params.get('returnUrl');
            }
          })
        }
      }
    })
  }
  ngOnInit() {
    this.initializeForm();
  }

  initializeForm() {
    this.loginForm = this.formBuilder.group({
      userName: new FormControl('', Validators.required),
      password: new FormControl('', Validators.required),
    });
  }

  login(){
    this.submitted = true;
    this.errorMessages = [];
    if(this.loginForm.valid){
      this.accountService.login(this.loginForm.value).subscribe({
        next: (response:any) => {
          if(this.returnUrl){
            this.router.navigateByUrl(this.returnUrl);
          }else{
            this.router.navigateByUrl('/');
          }
        },
        error: (error) => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
            console.log('Error messages:', this.errorMessages);
            this.loginForm.markAllAsTouched();
          }else{
            this.errorMessages.push(error.error);
          }
          this.cdr.detectChanges();
        }
      });
    }
  }

  resendEmailconfirmationLink(){
    this.router.navigateByUrl('/accounts/send-email/resend-email-confirmation-link')
  }

}
