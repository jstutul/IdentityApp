import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { Register } from '../shared/models/register';

@Injectable({
  providedIn: 'root',
})
export class Account {
  
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;
  constructor() {
    console.log('Account service initialized');
  }

  register(model:Register){
    return this.http.post(this.baseUrl + 'account/register', model);
  }
}
