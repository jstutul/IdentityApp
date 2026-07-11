import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet, RouterLinkActive } from '@angular/router';
import { Account } from '../services/account';
import { AsyncPipe } from '@angular/common';
import { pipe } from 'rxjs';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive,AsyncPipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  public accountService = inject(Account);
  constructor(){}
  logout(){
    this.accountService.logout();
  }
}
