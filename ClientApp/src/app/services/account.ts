import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Register } from '../shared/models/register';
import { Login } from '../shared/models/login';
import { User } from '../shared/models/user';
import { BehaviorSubject, map, of, ReplaySubject } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class Account {
  private userSource = new ReplaySubject<User|null>(1);
  user$ =this.userSource.asObservable();

  // private userSource = new BehaviorSubject<User | null>(null);
  // user$ = this.userSource.asObservable();

  private router = inject(Router);
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  constructor() {
    console.log('Account service initialized');
  }

  register(model:Register){
    return this.http.post(this.baseUrl + 'account/register', model);
  }
  login(model:Login){
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((user:User)=>{
        if(user){
          this.setUser(user);
          // return user;
        }
        // return null;
      })
    );
  }

  logout(){
    sessionStorage.removeItem(environment.userKey);
    this.userSource.next(null);
    this.router.navigateByUrl('/');
  }
  refreshUser(jwt:string|null){
    if(jwt===null){
      this.userSource.next(null);
      return of(undefined);
    }
    let headers = new HttpHeaders();
    headers = headers.set('Authorization','Bearer '+jwt)
    return this.http.get<User>(this.baseUrl + 'account/refresh-user-token',{headers}).pipe(
      map((user:User)=>{
        if(user){
          this.setUser(user);
        }
      })
    )
  }

  getJWT(){
    const key = localStorage.getItem(environment.userKey);
    if(key){
      const user:User=JSON.parse(key);
      return user.jwt;
    }else{
      return null;
    }
  }
  private setUser(user: User) {
    localStorage.setItem(environment.userKey, JSON.stringify(user));
    this.userSource.next(user);
  }
}
