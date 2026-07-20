import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './navbar/navbar';
import { Footer } from "./footer/footer";
import { Account } from './services/account';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Footer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  constructor(private accountService:Account){}
  ngOnInit(){
    this.refreshUser();
  }
  private refreshUser(){
    const jwt = this.accountService.getJWT();
    if(jwt){
      this.accountService.refreshUser(jwt).subscribe({
        next:_=>{

        },error:_=>{
          this.accountService.logout();
        }
      })
    }else{
      this.accountService.refreshUser(null).subscribe();
    }
  }


}
