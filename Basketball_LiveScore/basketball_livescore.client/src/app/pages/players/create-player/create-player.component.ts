import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service'
import { PlayerService } from '../../../services/player.service';

@Component({
  selector: 'app-create-player',
  templateUrl: './create-player.component.html',
  styleUrls: ['./create-player.component.css'],
})
export class CreatePlayerComponent implements OnInit {
  playerForm!: FormGroup;
  positionsOptions: { value: string; display: string }[] = [];
  teamsOptions: { value: number; display: string }[] = [];
  errorMessage: string | null = null;

  constructor(private fb: FormBuilder, private teamService: TeamService, private playerService: PlayerService) { }

  ngOnInit(): void {
    this.playerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(30)]],
      lastName: ['', [Validators.required, Validators.maxLength(30)]],
      number: ['', [Validators.required, Validators.min(0), Validators.max(99)]],
      height: ['', [Validators.required, Validators.min(1.50), Validators.max(2.50)]],
      position: ['', Validators.required],
      team: ['', Validators.required] 
    });

    this.loadTeams();
    this.loadPositions();
  }

  loadPositions(): void {
    this.playerService.getPositions().subscribe({
      next: (data) => {
        this.positionsOptions = data; // Assign fetched positions
      },
      error: (err) => {
        console.error('Error fetching positions:', err);
      }
    });
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.teamsOptions = data.map((team: any) => ({
          value: team.name, //la valeur sera le nom
          display: team.name //et on affiche aussi le nom
        }));
      },
      error: (err) => {
        console.error('Error fetching teams:', err);
      }
    });
  }


  get firstNameControl(): FormControl | null {
    return this.playerForm.get('firstName') as FormControl | null;
  }

  get lastNameControl(): FormControl | null {
    return this.playerForm.get('lastName') as FormControl | null;
  }

  get numberControl(): FormControl | null {
    return this.playerForm.get('number') as FormControl | null;
  }

  get heightControl(): FormControl | null {
    return this.playerForm.get('height') as FormControl | null;
  }

  get positionControl(): FormControl | null {
    return this.playerForm.get('position') as FormControl | null;
  }

  get teamControl(): FormControl | null {
    return this.playerForm.get('team') as FormControl | null;
  }

  onSubmit(): void {
    if (this.playerForm.valid) {
      const payload = {
        firstName: this.playerForm.value.firstName,
        lastName: this.playerForm.value.lastName,
        number: +this.playerForm.value.number, // avec le + va le convertir en nombre
        height: +this.playerForm.value.height, // pareil
        position: this.playerForm.value.position,
        team: this.playerForm.value.team
      };

      this.playerService.createPlayer(payload).subscribe({
        next: (response: any) => {
          console.log('Player created successfully:', response);
          alert('Player created successfully!');
          this.playerForm.reset(); // Clear the form
          this.errorMessage = null;
        },
        error: (error: any) => {
          console.error('Error creating player:', error);
          this.errorMessage = error.message;
        }
      });
    } else {
      this.errorMessage = "Please fill out the form correctly."
    }
  }
}
