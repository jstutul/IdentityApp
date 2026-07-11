import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [],
  templateUrl: './notification.html',
  styleUrl: './notification.css',
})
export class NotificationComponent {

  isSuccess: boolean = true;
  title: string = '';
  message: string = '';
  onOk?: () => void;

  constructor(public bsModalRef: BsModalRef) {}
  ok() {
    if (this.onOk) {
      this.onOk();
    }

    this.bsModalRef.hide();
  }
}
