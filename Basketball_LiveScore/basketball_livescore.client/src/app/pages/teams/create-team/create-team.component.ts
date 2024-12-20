import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service';

@Component({
  selector: 'app-create-team',
  templateUrl: './create-team.component.html',
  styleUrls: ['./create-team.component.css']
})
export class CreateTeamComponent implements OnInit {
  teamForm!: FormGroup;
  errorMessage: string | null = null; //sera null ou vide au debut
  constructor(private fb: FormBuilder, private teamService: TeamService) { }

  ngOnInit(): void {
    // Initialize the form with validation
    this.teamForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(30)]],
      coach: ['', [Validators.required, Validators.maxLength(50)]],
    });
  }

  get nameControl(): FormControl | null {
    return this.teamForm.get('name') as FormControl | null;
  }

  get coachControl(): FormControl | null {
    return this.teamForm.get('coach') as FormControl | null;
  }

  onSubmit(): void {
    if (this.teamForm.valid) {
      this.teamService.createTeam(this.teamForm.value).subscribe({
        next: (response: any) => {
          alert('Team created successfully!');
          this.teamForm.reset(); //apres avoir rempli le form on le remet a zero
          this.errorMessage = null;
        },
        error: (error: any) => {
          console.error('Error creating team:', error);
          this.errorMessage = error.message; 
        },
      });
    } else {
      this.errorMessage = "Please fill out the form correctly."
    }
  }
}
