import { Component, inject } from '@angular/core';
import { PlayService } from '../services/play';

@Component({
  selector: 'app-play',
  imports: [],
  templateUrl: './play.html',
  styleUrl: './play.css',
})
export class Play {
  private playService = inject(PlayService);
  message : string|undefined;

  ngOnInit(){
    this.playService.getPlayers().subscribe({
      next:(response:any)=>{
        this.message=response.value.message;
        console.log(response);
      },
      error:(e)=>{
        console.log(e);
      }
    })
  }
}
