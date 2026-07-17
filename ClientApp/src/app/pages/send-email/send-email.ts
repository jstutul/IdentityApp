import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Shared } from '../../services/shared';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from '../../services/account';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { ValidationMessages } from "../../shared/components/errors/validation-messages/validation-messages";

@Component({
  selector: 'app-send-email',
  imports: [ReactiveFormsModule, ValidationMessages],
  templateUrl: './send-email.html',
  styleUrl: './send-email.css',
})
export class SendEmail {
  emailForm : FormGroup =new FormGroup({});
  submitted =false;
  mode:string|undefined;
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
            const mode = this.activeRoute.snapshot.paramMap.get('mode');
            if(mode){
              this.mode=mode;
              this.initializeForm();
            }
          }
        }
      })
  }
  initializeForm(){
    this.emailForm  = this.formBuilder.group({
      email: ['',[Validators.required,Validators.pattern(/^[^\s@]+@[^\s@]+\.[^\s@]+$/)]],
    });
  }


  sendEmail(){
    this.submitted=true;
    this.errorMessages=[];
    if(this.emailForm.valid && this.mode){
      if(this.mode.includes('resend-email-confirmation-link')){
        this.accountService.resendEmailConfirmationLink(this.emailForm.get('email')?.value).subscribe({
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
      else if(this.mode.includes('forget-username-or-password')){
        this.accountService.forgotUsernameOrPassword(this.emailForm.get('email')?.value).subscribe({
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
  cancel(){
    this.router.navigateByUrl('accounts/login');
  }

}
