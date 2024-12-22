import { Component, OnInit, OnDestroy } from '@angular/core';
import { PlayerMatch,Team,MatchState,SubstitutionEvent,ScoreEvent,FoulEvent,TimeoutEvent } from '../../../models/interfaces';

@Component({
  selector: 'app-update-match',
  templateUrl: './update-match.component.html',
  styleUrls: ['./update-match.component.css']
})
export class UpdateMatchComponent implements OnInit, OnDestroy {
  quarter: number = 1;
  time: number = 600;
  isRunning: boolean = false;
  timeoutTime: number = 0;
  isTimeout: boolean = false;
  timerInterval: any;
  selectedPlayerForSub: PlayerMatch | null = null;
  substitutionActive: boolean = false;
  activeTeam: Team | null = null;
  playerToSubOut: PlayerMatch | null = null;

  team1: Team = {
    name: 'Lakers',
    score: 0,
    timeouts: 4,
    players: [
      { id: 1, number: 5, name: "John Smith", fouls: 0, points: 0, onCourt: true },
      { id: 2, number: 7, name: "Mike Johnson", fouls: 0, points: 0, onCourt: true },
      { id: 3, number: 10, name: "Steve Williams", fouls: 0, points: 0, onCourt: true },
      { id: 4, number: 15, name: "James Davis", fouls: 0, points: 0, onCourt: true },
      { id: 5, number: 23, name: "Tom Wilson", fouls: 0, points: 0, onCourt: true },
      { id: 6, number: 8, name: "Dan Brown", fouls: 0, points: 0, onCourt: false },
      { id: 7, number: 12, name: "Chris Lee", fouls: 0, points: 0, onCourt: false },
      { id: 8, number: 21, name: "Paul Martin", fouls: 0, points: 0, onCourt: false }
    ],
    coach: 'Red Auerbach',
  };

  team2: Team = {
    name: 'Celtics',
    score: 0,
    timeouts: 4,
    players: [
      { id: 9, number: 4, name: "Kevin Thompson", fouls: 0, points: 0, onCourt: true },
      { id: 10, number: 6, name: "Mark Anderson", fouls: 0, points: 0, onCourt: true },
      { id: 11, number: 11, name: "Ryan Taylor", fouls: 0, points: 0, onCourt: true },
      { id: 12, number: 14, name: "David Moore", fouls: 0, points: 0, onCourt: true },
      { id: 13, number: 20, name: "Brian White", fouls: 0, points: 0, onCourt: true },
      { id: 14, number: 9, name: "Eric Clark", fouls: 0, points: 0, onCourt: false },
      { id: 15, number: 16, name: "Adam Hall", fouls: 0, points: 0, onCourt: false },
      { id: 16, number: 25, name: "Jason Green", fouls: 0, points: 0, onCourt: false }
    ],
    coach: 'Red PAsRed',
  };

  constructor() { }

  ngOnInit() {
    this.startTimer();
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  private startTimer() {
    this.timerInterval = setInterval(() => {
      if (this.isRunning && this.time > 0) {
        this.time--;
      } else if (this.isTimeout && this.timeoutTime > 0) {
        this.timeoutTime--;
        if (this.timeoutTime === 0) {
          this.endTimeout();
        }
      }
    }, 1000);
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  toggleTimer() {
    this.isRunning = !this.isRunning;
  }

  nextQuarter() {
    if (this.quarter < 4) {
      this.quarter++;
      this.time = 600;
      this.isRunning = false;
    }
  }

  handleScoreChange(team: Team, player: PlayerMatch, points: number) {
    team.score += points;
    player.points += points;
  }

  handleFoul(player: PlayerMatch) {
    player.fouls++;
  }

  startTimeout(team: Team) {
    if (team.timeouts > 0) {
      this.isRunning = false;
      this.isTimeout = true;
      this.timeoutTime = 30;
      team.timeouts--;
    }
  }

  endTimeout() {
    this.isTimeout = false;
    this.timeoutTime = 0;
    this.isRunning = true;
  }

  initiateSubstitution(team: Team, player: PlayerMatch) {
    this.substitutionActive = true;
    this.activeTeam = team;
    this.playerToSubOut = player;
  }

  completeSubstitution(playerIn: PlayerMatch): void {
    if (this.activeTeam && this.playerToSubOut) {
      this.activeTeam.players = this.activeTeam.players.map(p => ({
        ...p,
        onCourt: p.id === this.playerToSubOut?.id ? false :
          p.id === playerIn.id ? true :
            p.onCourt
      }));
    }
    this.cancelSubstitution();
  }

  cancelSubstitution(): void {
    this.substitutionActive = false;
    this.activeTeam = null;
    this.playerToSubOut = null;
  }

  handleSubstitution(team: Team, playerIn: PlayerMatch) {
    if (this.selectedPlayerForSub) {
      team.players = team.players.map(p => ({
        ...p,
        onCourt: p.id === this.selectedPlayerForSub?.id ? false :
          p.id === playerIn.id ? true :
            p.onCourt
      }));
      this.selectedPlayerForSub = null;
    }
  }

  getOnCourtPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => p.onCourt);
  }

  getBenchPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => !p.onCourt);
  }
}
