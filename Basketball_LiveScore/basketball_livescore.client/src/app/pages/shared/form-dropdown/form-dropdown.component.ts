import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-form-dropdown',
  templateUrl: './form-dropdown.component.html',
  styleUrls: ['./form-dropdown.component.css'],
})
export class FormDropdownComponent {
  @Input() label!: string;
  @Input() options!: { value: any; display: string }[];
  @Input() control!: FormControl;
  @Input() errorMessage!: string;
}
