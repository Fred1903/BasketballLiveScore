import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-form-input',
  templateUrl: './form-input.component.html',
  styleUrls: ['./form-input.component.css'],
})
export class FormInputComponent {
  @Input() label!: string;
  @Input() placeholder!: string;
  @Input() control!: FormControl;
  @Input() type: string = 'text';
  @Input() errorMessage!: string;
}
