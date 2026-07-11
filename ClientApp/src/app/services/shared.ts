import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { NotificationComponent } from '../shared/notification/notification';

@Injectable({
  providedIn: 'root',
})
export class Shared {
  bsModalRef?: BsModalRef;
  constructor(private modalService:BsModalService) {

  }

  showNofication(isSuccess: boolean, title: string, message: string, onOk?: () => void) {
    console.log('showNofication called with:', { isSuccess, title, message });
    const initialState: ModalOptions = {
      initialState: {
        isSuccess,
        title,
        message,
        onOk
      }
    };
    this.modalService.show(NotificationComponent,  initialState);
  }
}
