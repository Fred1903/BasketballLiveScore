import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-create-player',
  templateUrl: './create-player.component.html',
  styleUrls: ['./create-player.component.css']
})
export class CreatePlayerComponent implements OnInit {
  playerForm!: FormGroup;
  positions: string[] = ['Guard', 'Forward', 'Center']; // Example positions
  teams: any[] = [
    { id: 1, name: 'Team A' },
    { id: 2, name: 'Team B' },
    { id: 3, name: 'Team C' }
  ]; // Example teams

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    // Initialize the form with default values and validation
    this.playerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(30)]],
      lastName: ['', [Validators.required, Validators.maxLength(30)]],
      number: ['', [Validators.required, Validators.min(0), Validators.max(99)]],
      position: ['', Validators.required],
      height: ['', [Validators.required, Validators.min(100), Validators.max(300)]],
      teamId: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.playerForm.valid) {
      console.log('Player Created:', this.playerForm.value);
      // Add API call logic here to send the form data to the backend
    } else {
      console.log('Form is invalid');
    }
  }
}
