import { Component, inject, OnInit } from '@angular/core';
import { Account } from '../../services/account';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { confirmEmail } from '../../shared/models/account/confirmEmail';
import { Shared } from '../../services/shared';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.css',
})
export class ConfirmEmail {
  private accountService= inject(Account);
  private router =inject(Router);
  private activeRoute = inject(ActivatedRoute);
  private sharedService =inject(Shared);
  success =true;
  constructor() { 
}

ngOnInit() {
  this.accountService.user$.pipe(take(1)).subscribe({
    next:(user:User|null)=>{
      if(user){
        this.router.navigateByUrl('/');
      }else{
        console.log(11);
        this.activeRoute.queryParamMap.subscribe({
          next:(params:any)=>{
            const confirmEmail:confirmEmail={
              email:params.get('email'),
              token:params.get('token')
            }
            this.accountService.confirmEmail(confirmEmail).subscribe({
              next:(response:any)=>{
                this.sharedService.showNofication(true,response.value.title,response.value.message);
              },error:error=>{
                this.success=false;
                this.sharedService.showNofication(false,"Failed",error.error);
              }
            });
          }
        })
      }
    }
  })
}
resendEmailConfirmation(){
  this.router.navigateByUrl('accounts/send-email/resend-email-confirmation-link');
}

}
