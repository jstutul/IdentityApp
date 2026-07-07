import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-validation-messages',
  standalone: true,
  templateUrl: './validation-messages.html',
  styleUrl: './validation-messages.css'
})
export class ValidationMessages {
  @Input() errorMessages: string[] = [];
}