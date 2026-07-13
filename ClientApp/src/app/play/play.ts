import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { PlayService } from '../services/play';

@Component({
  selector: 'app-play',
  imports: [],
  templateUrl: './play.html',
  styleUrl: './play.css',
})
export class Play implements OnInit {
  private playService = inject(PlayService);
  private cdr = inject(ChangeDetectorRef);
  message : string|undefined;

  ngOnInit(): void {
    this.playService.getPlayers().subscribe({
      next: (response: any) => {
        console.log(response.value.message);
        this.message = response.value.message;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error(err);
      }
    });
  }
}
