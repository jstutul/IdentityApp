import { ChangeDetectorRef, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from '../../services/account';
import { Shared } from '../../services/shared';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { LoginWithExternal } from '../../shared/models/account/LoginWithExternal';
import { jwtDecode } from 'jwt-decode';
import { GoogleJwtPayload } from '../../shared/models/account/GoogleJwtPayload ';
declare const FB:any;
declare const google: any;

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, ValidationMessages, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  @ViewChild('googleButton')
  googleButton!: ElementRef<HTMLDivElement>;

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
     FB.init({
      appId: '1682541726343840', 
      cookieDomain: 'all',
      version: 'v18.0'
    });
  }

  ngAfterViewInit() {
    this.initializeGoogleButton();
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
        next: _ => {
          if(this.returnUrl){
            this.router.navigateByUrl(this.returnUrl);
          }else{
            this.router.navigateByUrl('/');
          }
        },
        error: (error) => {
          if (error.error.errors) {
            this.errorMessages = error.error.errors;
            this.loginForm.markAllAsTouched();
          }else{
            this.errorMessages.push(error.error);
          }
          this.cdr.detectChanges();
        }
      });
    }
  }

  loginWithFacebook(){
  FB.login((fbResult: any) => {
      if (fbResult.authResponse) {
        const accessToken = fbResult.authResponse.accessToken;
        const userId = fbResult.authResponse.userID;
        this.accountService.loginWithThirdParty(new LoginWithExternal(userId,accessToken,"facebook")).subscribe({
          next:_=>{
            if(this.returnUrl){
            this.router.navigateByUrl(this.returnUrl);
            }else{
              this.router.navigateByUrl('/');
            }
          },
          error: (error) => {
            if (error.error.errors) {
              this.errorMessages = error.error.errors;
              this.loginForm.markAllAsTouched();
            }else{
              this.errorMessages.push(error.error);
            }
            this.cdr.detectChanges();
          }
        })
      } else {
        this.sharedService.showNofication(false,"Failed","Unable to login with your facebook");
      }
    }, { scope: 'public_profile,email' }); // Added scope to get email
  }

  resendEmailconfirmationLink(){
    this.router.navigateByUrl('/accounts/send-email/resend-email-confirmation-link')
  }

  private initializeGoogleButton() {
      google.accounts.id.initialize({
        client_id:'233596855582-97av92bdkih4r1fcu4dcddrcdmlenqib.apps.googleusercontent.com',
        callback: this.googleCallback.bind(this),
        auto_select: false
      });
  
      google.accounts.id.renderButton(
        this.googleButton.nativeElement,
        {
          // theme: 'outline',
          size: 'medium',
          text: 'signup_with',
          shape: 'rectangular',
          logo_alignment:'center'
        }
      );
      google.accounts.id.prompt();
    }
  
    private async googleCallback(response: any) {
      const decodedToken = jwtDecode<GoogleJwtPayload>(response.credential);
      this.accountService.loginWithThirdParty(new LoginWithExternal(decodedToken.sub,response.credential,"google")).subscribe({
          next:_=>{
            if(this.returnUrl){
            this.router.navigateByUrl(this.returnUrl);
            }else{
              this.router.navigateByUrl('/');
            }
          },
          error: (error) => {
            if (error.error.errors) {
              this.errorMessages = error.error.errors;
              this.loginForm.markAllAsTouched();
            }else{
              this.errorMessages.push(error.error);
            }
            this.cdr.detectChanges();
          }
        })
    }
  

}
