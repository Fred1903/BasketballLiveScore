import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service';
import { PlayerService } from '../../../services/player.service';
import { UserService } from '../../../services/user.service';
import { MatchService } from '../../../services/match.service';
import { AuthService } from '../../../services/auth.service';

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
  encodersOptions: { value: number; display: string }[] = [];

  constructor(private fb: FormBuilder, private teamService: TeamService, private playerService: PlayerService
    , private userService: UserService, private matchService: MatchService, private authService: AuthService) { }

  ngOnInit(): void {
    this.matchForm = this.fb.group({
      team1: [null, Validators.required],
      team2: [null, Validators.required],
      numberOfQuarters: [null, Validators.required],
      quarterDuration: [null, Validators.required],
      timeoutDuration: [null, Validators.required],
      encoder: [null, Validators.required],
    });

    this.loadTeams();
    this.loadSettings();
    this.loadEncoders();
  }

  loadEncoders(): void {
    this.userService.getUsersByRole('Admin').subscribe({
      next: (users) => {
        console.log("users : " + users)
        console.log("1er user", users[0])
        console.log("1er userr:", JSON.stringify(users[0], null, 2));

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
        console.log("def : " + defaults)
        console.log("valeur par défsut : " + defaults.NumberOfQuarters + "," + defaults.QuarterDuration + "," + defaults.TimeoutDuration)
        this.matchForm.patchValue({
          numberOfQuarters: defaults.NumberOfQuarters,
          quarterDuration: defaults.QuarterDuration,
          timeoutDuration: defaults.TimeoutDuration,
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
    console.log("dans le onchangeeeeee")
    const teamId = teamNumber === 1 ? this.team1Control?.value : this.team2Control?.value;
    if (teamId) {
      this.teamService.getPlayersByTeam(teamId).subscribe({
        next: (players) => {
          console.log("dans next ")
          if (teamNumber === 1) {
            this.team1Players = players;
            console.log("players de 1 : " + players)
          } else {
            this.team2Players = players;
            console.log("players de 2 : " + players)
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
      if (selectedSet.size >= 12) {
        alert('You can only select up to 12 players.');
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
      const payload = {
        team1: this.matchForm.value.team1,
        team2: this.matchForm.value.team2,
        numberOfQuarters: this.matchForm.value.numberOfQuarters,
        quarterDuration: this.matchForm.value.quarterDuration,
        timeoutDuration: this.matchForm.value.timeoutDuration,//Le encoder settings est user connecté
        encoderSettings: userInfo ? userInfo.id : null,
        encoderRealTime: this.matchForm.value.encoder,
        playersTeam1: Array.from(this.selectedTeam1Players),
        playersTeam2: Array.from(this.selectedTeam2Players),
        startersTeam1: Array.from(this.startersTeam1),
        startersTeam2: Array.from(this.startersTeam2),
      };

      console.log('Match Payload:', payload);

      this.matchService.createMatch(payload).subscribe({
        next: (response) => {
          console.log('Match created successfully:', response);
          alert('Match created successfully!');
          this.matchForm.reset();
        },
        error: (err) => {
          console.error('Error creating match:', err);
          alert('Failed to create match. Please try again.');
        },
      });
    } else {
      alert('Please fill out the form correctly.');
    }
  }
}
