import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service';
import { PlayerService } from '../../../services/player.service';
import { UserService } from '../../../services/user.service';
import { MatchService } from '../../../services/match.service';
import { AuthService } from '../../../services/auth.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-create-match',
  templateUrl: './create-match.component.html',
  styleUrls: ['./create-match.component.css'],
})
export class CreateMatchComponent implements OnInit {
  matchForm!: FormGroup;
  teamsOptions: { value: number; display: string }[] = [];
  team1Players: any[] = [];
  team2Players: any[] = [];
  selectedTeam1Players: Set<number> = new Set();
  selectedTeam2Players: Set<number> = new Set();
  startersTeam1: Set<number> = new Set();
  startersTeam2: Set<number> = new Set();
  numberOfQuartersOptions: number[] = [];
  quarterDurationOptions: number[] = [];
  timeoutDurationOptions: number[] = [];
  timeoutAmountOptions: number[] = [];
  encodersOptions: { value: number; display: string }[] = [];
  minDate!: string;
  minTime!: string;
  errorMessage: string | null = null; 

  constructor(private fb: FormBuilder, private teamService: TeamService, private playerService: PlayerService
    , private userService: UserService, private matchService: MatchService, private authService: AuthService, private datePipe: DatePipe)
  {
    const today = new Date();
    this.minDate = this.datePipe.transform(today, 'yyyy-MM-dd')!;
    this.minTime = this.datePipe.transform(today, 'HH:mm')!;
  }

  ngOnInit(): void {
    this.matchForm = this.fb.group({
      team1: [null, Validators.required],
      team2: [null, Validators.required],
      numberOfQuarters: [null, Validators.required],
      quarterDuration: [null, Validators.required],
      timeoutDuration: [null, Validators.required],
      timeoutAmount: [null, Validators.required],
      encoder: [null, Validators.required],
      matchDate: [null, Validators.required],
      matchTime: [null, Validators.required]
    });

    this.loadTeams();
    this.loadSettings();
    this.loadEncoders();
  }

  loadEncoders(): void {
    this.userService.getUsersByRole('Admin').subscribe({
      next: (users) => {
        this.encodersOptions = users.map((user: any) => ({
          value: user.id,
          display: user.firstName + " " + user.lastName,
        }));

      },
      error: (err) => {
        console.error('Error fetching encoders:', err);
      },
    });
  }

  loadSettings(): void {
    //On met les valeurs par défaut du back-end d'abord
    this.matchService.getDefaultSettings().subscribe({
      next: (defaults) => {
        this.matchForm.patchValue({
          numberOfQuarters: defaults.NumberOfQuarters,
          quarterDuration: defaults.QuarterDuration,
          timeoutDuration: defaults.TimeoutDuration,
          timeoutAmount: defaults.TimeoutAmount
        });
      },
      error: (err) => {
        console.error('Error fetching default match settings:', err);
      },
    });
    this.matchService.getNumberOfQuartersOptions().subscribe({
      next: (data) => {
        this.numberOfQuartersOptions = data;
      },
      error: (err) => {
        console.error('Error fetching number of quarters options:', err);
      },
    });
    this.matchService.getQuarterDurationOptions().subscribe({
      next: (data) => {
        this.quarterDurationOptions = data;
      },
      error: (err) => {
        console.error('Error fetching quarter duration options:', err);
      },
    });
    this.matchService.getTimeoutDurationOptions().subscribe({
      next: (data) => {
        this.timeoutDurationOptions = data;
      },
      error: (err) => {
        console.error('Error fetching timeout duration options:', err);
      },
    });
    this.matchService.getTimeoutAmountOptions().subscribe({
      next: (data) => {
        this.timeoutAmountOptions = data;
      },
      error: (err) => {
        console.error('Error fetching timeout amount options:', err);
      },
    });
  }


  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.teamsOptions = data.map((team: any) => ({
          value: team.id,
          display: team.name,
        }));
      },
      error: (err) => {
        console.error('Error fetching teams:', err);
      },
    });
  }

  onTeamChange(teamNumber: number): void {
    const teamId = teamNumber === 1 ? this.team1Control?.value : this.team2Control?.value;
    if (teamId) {
      this.teamService.getPlayersByTeam(teamId).subscribe({
        next: (players) => {
          if (teamNumber === 1) {
            this.team1Players = players;
          } else {
            this.team2Players = players;
          }
        },
        error: (err) => {
          console.error(`Error fetching players for team ${teamNumber}:`, err);
        },
      });
    }
  }

  togglePlayer(playerId: number, teamNumber: number): void {
    const selectedSet = teamNumber === 1 ? this.selectedTeam1Players : this.selectedTeam2Players;
    if (selectedSet.has(playerId)) {
      selectedSet.delete(playerId);
    } else {
      if (selectedSet.size >= 13) {
        alert('You can only select up to 13 players.');
        return;
      }
      selectedSet.add(playerId);
    }
  }

  toggleStarter(playerId: number, teamNumber: number): void {
    const starterSet = teamNumber === 1 ? this.startersTeam1 : this.startersTeam2;
    if (starterSet.has(playerId)) {
      starterSet.delete(playerId);
    } else {
      starterSet.add(playerId);
    }
  }

  get team1Control(): FormControl | null {
    return this.matchForm.get('team1') as FormControl | null;
  }

  get team2Control(): FormControl | null {
    return this.matchForm.get('team2') as FormControl | null;
  }

  get encoderControl(): FormControl | null{
    return this.matchForm.get('encoder') as FormControl | null;
  }

  onSubmit(): void {
    if (this.matchForm.valid) {
      const userInfo = this.authService.getUserInfo();
      const matchDateTime = new Date(
        this.matchForm.value.matchDate + 'T' + this.matchForm.value.matchTime
      );
      const payload = {
        team1: this.matchForm.value.team1,
        team2: this.matchForm.value.team2,
        numberOfQuarters: this.matchForm.value.numberOfQuarters,
        quarterDuration: this.matchForm.value.quarterDuration,
        timeoutDuration: Number(this.matchForm.value.timeoutDuration),
        timeoutAmount: Number(this.matchForm.value.timeoutAmount),
        encoderSettingsId: userInfo ? userInfo.id : null,//Le encoder settings est user connecté
        encoderRealTimeId: this.matchForm.value.encoder,
        playersTeam1: Array.from(this.selectedTeam1Players),
        playersTeam2: Array.from(this.selectedTeam2Players),
        startersTeam1: Array.from(this.startersTeam1),
        startersTeam2: Array.from(this.startersTeam2),
        matchDate: matchDateTime.toISOString()
      };
      this.matchService.createMatch(payload).subscribe({
        next: (response) => {
          alert('Match created successfully!');
          this.matchForm.reset();
          this.errorMessage = null;
        },
        error: (error: any) => {
          console.log("le message d'erreur : " + error.message)
          this.errorMessage = error.message; 
        },
      });
    } else {
      alert('Please fill out the form correctly.');
    }
  }
}
