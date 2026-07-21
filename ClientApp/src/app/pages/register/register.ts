import { ChangeDetectorRef, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Account } from '../../services/account';
import { ValidationMessages } from '../../shared/components/errors/validation-messages/validation-messages';
import { Shared } from '../../services/shared';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import { User } from '../../shared/models/account/user';
import { jwtDecode } from 'jwt-decode';
declare const FB:any;
declare const google: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, ValidationMessages],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  @ViewChild('googleButton')
  googleButton!: ElementRef<HTMLDivElement>;
  registerForm: FormGroup = new FormGroup({});
  submitted = false;
  errorMessages:string[] = [];
  private accountService = inject(Account);
  private sharedService = inject(Shared);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  constructor(private formBuilder: FormBuilder) {
    this.accountService.user$.pipe(take(1)).subscribe({
      next:(user:User|null)=>{
        if(user){
          this.router.navigateByUrl('/');
        }
      }
    })
  }
  ngOnInit() {
    this.initializeForm();
    //this.initializeGoogleButton();
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
    if(this.registerForm.valid){
      this.accountService.register(this.registerForm.value).subscribe({
        next: (response:any) => {
        this.sharedService.showNofication(
          true,
          response.value.title,
          response.value.message,
          () => {
            this.router.navigate(['/accounts/login']);
          }
        );
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

  registerWithFacebook(){
    FB.login((fbResult: any) => {
      if (fbResult.authResponse) {
        const accessToken = fbResult.authResponse.accessToken;
        const userId = fbResult.authResponse.userID;
        this.router.navigateByUrl(`/accounts/register/third-party/facebook?access_token=${accessToken}&userId=${userId}`);
        //this.processFacebookLogin(fbResult.authResponse);
      } else {
        this.sharedService.showNofication(false,"Failed","Unable to register with your facebook");
      }
    }, { scope: 'public_profile,email' }); // Added scope to get email
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
    const decodedToken = jwtDecode(response.credential);
    this.router.navigateByUrl(`/accounts/register/third-party/google?access_token=${response.credential}&userId=${decodedToken.sub}`);
  }

  async processFacebookLogin(authResponse: any) {
    const accessToken = authResponse.accessToken;
    
    // Now you can use await here!
    FB.api('/me', { fields: 'id,name,email' }, (response: any) => {
      console.log('User Data: ', response);
      // Call your backend service here
    });
  }

}
