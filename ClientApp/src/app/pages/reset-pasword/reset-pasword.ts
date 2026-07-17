import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Account } from '../../services/account';
import { Shared } from '../../services/shared';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';
import { User } from '../../shared/models/account/user';
import { take } from 'rxjs';
import { ResetPassword } from '../../shared/models/account/ResetPassword';

@Component({
  selector: 'app-reset-pasword',
  imports: [ReactiveFormsModule, ValidationMessages, RouterLink],
  templateUrl: './reset-pasword.html',
  styleUrl: './reset-pasword.css',
})
export class ResetPasword {

  resetPasswordForm : FormGroup =new FormGroup({});
  token:string | undefined;
  email:string | undefined;
  submitted =false;
  errorMessages:string[]=[];

  private accountService= inject(Account);
  private router =inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private sharedService =inject(Shared);
  private formBuilder= inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  ngOnInit(){
    this.accountService.user$.pipe(take(1)).subscribe({
        next:(user:User|null)=>{
          if(user){
            this.router.navigateByUrl('/');
          }else{
            this.activeRoute.queryParamMap.subscribe({
              next:(params:any)=>{
                this.token = params.get('token');
                this.email = params.get('email');
                if(this.token && this.email){
                  this.initializeForm(this.email);
                }else{
                  this.router.navigateByUrl('/accounts/login');
                }
              }
            })
          }
        }
      })
  }

  initializeForm(username:string){
    this.resetPasswordForm  = this.formBuilder.group({
      email: [{value:username,disabled:true}],
      newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(15)]], 
    });
  }

  resetPassword(){
    this.submitted=true;
    this.errorMessages=[];
    if(this.resetPasswordForm.valid && this.token && this.email){
      const model : ResetPassword ={
        email:this.email,
        token:this.token,
        newPassword:this.resetPasswordForm.get('newPassword')?.value
      };
      console.log(model);
      this.accountService.resetPassword(model).subscribe({
        next:(response:any)=>{
            this.sharedService.showNofication(true,
              response.value.title,
              response.value.message,
              () => {
                this.router.navigate(['/accounts/login']);
              }
            );
          },
          error: error => {
            if (error.error.errors) {
              this.errorMessages = error.error.errors;
            }else{
              this.errorMessages.push(error.error);
            }
            this.cdr.detectChanges();
          }
      })
    }
  }
}
