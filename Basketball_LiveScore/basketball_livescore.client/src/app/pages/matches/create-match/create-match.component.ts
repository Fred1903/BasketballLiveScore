import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service';
import { PlayerService } from '../../../services/player.service';

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

  constructor(private fb: FormBuilder, private teamService: TeamService, private playerService: PlayerService) { }

  ngOnInit(): void {
    this.matchForm = this.fb.group({
      team1: [null, Validators.required],
      team2: [null, Validators.required],
    });

    this.loadTeams();
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

  onSubmit(): void {
    if (this.matchForm.valid) {
      const payload = {
        team1: this.matchForm.value.team1,
        team2: this.matchForm.value.team2,
        playersTeam1: Array.from(this.selectedTeam1Players).map((id) => ({
          id,
          isStarter: this.startersTeam1.has(id),
        })),
        playersTeam2: Array.from(this.selectedTeam2Players).map((id) => ({
          id,
          isStarter: this.startersTeam2.has(id),
        })),
      };

      console.log('Match Payload:', payload);

      // API call to create match
      // matchService.createMatch(payload).subscribe(...)
    } else {
      alert('Please fill out the form correctly.');
    }
  }
}
