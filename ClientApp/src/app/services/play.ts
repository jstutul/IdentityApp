import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class PlayService {
  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getPlayers(){
    return this.http.get(this.baseUrl+'play/get-players');  
  }
}
