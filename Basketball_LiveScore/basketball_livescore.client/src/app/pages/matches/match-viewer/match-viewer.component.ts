import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatchService } from '../../../services/match.service';
import { MatchSettingsService } from '../../../services/match-settings.service'
import {
  PlayerMatch,
  Team,
  MatchDetails,
  BasketEvent,
  FoulEvent,
  TimeoutEvent,
  SubstitutionEvent,
  QuarterChangeEvent,
  ChronoEvent,
  RecentEvent
} from '../../../models/interfaces';

@Component({
  selector: 'app-match-viewer',
  templateUrl: './match-viewer.component.html',
  styleUrls: ['./match-viewer.component.css']
})
export class MatchViewerComponent implements OnInit, OnDestroy {
  matchStatus: 'not_started' | 'in_progress' | 'timeout' | 'quarter_break' | 'finished' = 'not_started';
  quarter: number = 1;
  time: number = 600;
  isTimeout: boolean = false;
  timeoutTime: number = 0;
  matchDetails: MatchDetails | null = null;
  idMatch: number = 2;
  recentEvents: RecentEvent[] = [];
  private intervalId: any;
  private timerInterval: any;
  isRunning: boolean = false;

  team1: Team = {
    name: '',
    score: 0,
    timeouts: 4,
    players: [],
    coach: ''
  };

  team2: Team = {
    name: '',
    score: 0,
    timeouts: 4,
    players: [],
    coach: ''
  };

  constructor(
    private matchService: MatchService,
    private route: ActivatedRoute,
    private matchSettingsService: MatchSettingsService
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = parseInt(params['id']);
      if (!isNaN(id)) {  // Add check to ensure id is a valid number
        this.idMatch = id;
        this.loadMatchDetails(this.idMatch);
        this.matchService.joinMatchGroup(this.idMatch);
        this.subscribeToMatchEvents();
      }
    });
  }

  ngOnDestroy() {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  private loadMatchDetails(matchId: number): void {
    this.matchSettingsService.getMatchDetails(matchId).subscribe({
      next: (data) => {
        this.matchDetails = data;
        this.initializeTeams(data);
        // Set initial match status based on data
        this.determineMatchStatus(data);
      },
      error: (err) => console.error('Error loading match details:', err)
    });
  }

  private determineMatchStatus(data: MatchDetails): void {
    const currentTime = new Date();
    const matchTime = new Date(data.matchDate);

    if (currentTime < matchTime) {
      this.matchStatus = 'not_started';
    } else {
      this.matchStatus = 'in_progress';
    }
  }

  private initializeTeams(data: MatchDetails): void {
    this.team1 = {
      name: data.homeTeam.name,
      score: data.scoreHome,
      timeouts: data.timeouts,
      players: data.players
        .filter(p => p.isHomeTeam)
        .map(player => ({
          id: player.playerId,
          number: player.playerNumber,
          name: player.playerName,
          fouls: 0,
          points: 0,
          onCourt: player.isStarter
        })),
      coach: data.homeTeam.coach
    };

    this.team2 = {
      name: data.awayTeam.name,
      score: data.scoreAway,
      timeouts: data.timeouts,
      players: data.players
        .filter(p => !p.isHomeTeam)
        .map(player => ({
          id: player.playerId,
          number: player.playerNumber,
          name: player.playerName,
          fouls: 0,
          points: 0,
          onCourt: player.isStarter
        })),
      coach: data.awayTeam.coach
    };
  }

  private startMatchTimer(): void {
    this.intervalId = setInterval(() => {
      if (this.matchStatus === 'in_progress' && !this.isTimeout) {
        if (this.time > 0) {
          this.time--;
        } else {
          // Quarter ended
          if (this.quarter < 4) {
            this.matchStatus = 'quarter_break';
          } else {
            this.matchStatus = 'finished';
            clearInterval(this.intervalId);
          }
        }
      } else if (this.isTimeout && this.timeoutTime > 0) {
        this.timeoutTime--;
        if (this.timeoutTime === 0) {
          this.isTimeout = false;
          this.matchStatus = 'in_progress';
        }
      }
    }, 1000);
  }

  private subscribeToMatchEvents(): void {
    this.matchService.joinMatchGroup(this.idMatch);

    this.matchService.subscribeToBasketEvents((event) => {
      this.handleBasketEvent(event);
    });

    this.matchService.subscribeToFoulEvents((event) => {
      this.handleFoulEvent(event);
    });

    this.matchService.subscribeToTimeoutEvents((event) => {
      this.handleTimeoutEvent(event);
    });

    this.matchService.subscribeToSubstitutionEvents((event) => {
      this.handleSubstitutionEvent(event);
    });

    this.matchService.subscribeToQuarterChangeEvents((event) => {
      this.handleQuarterChangeEvent(event);
    });

    this.matchService.subscribeToChronoEvents((event) => {
      this.handleChronoEvent(event);
    });
  }

  // Event Handlers
  private handleBasketEvent(event: BasketEvent): void {
    if (this.idMatch === event.matchId) {
      const player = this.findPlayerById(event.playerId);
      if (player) {
        player.points += event.points;
        const team = this.getTeamByPlayerId(event.playerId);
        if (team) {
          team.score += event.points;
        }
        this.addRecentEvent('score', `${player.name} scores ${event.points} points`);
      }
    }
  }

  private handleFoulEvent(event: FoulEvent): void {
    if (this.idMatch === event.matchId) {
      const player = this.findPlayerById(event.playerId);
      if (player) {
        player.fouls++;
        this.addRecentEvent('foul', `Foul on ${player.name} (${event.foulType})`);
      }
    }
  }

  private handleTimeoutEvent(event: TimeoutEvent): void {
    if (this.idMatch === event.matchId) {
      const team = event.team === 'Home' ? this.team1 : this.team2;
      if (team && team.timeouts > 0) {
        team.timeouts--;
        this.isTimeout = true;
        this.timeoutTime = 30;
        this.matchStatus = 'timeout';
        this.addRecentEvent('timeout', `Timeout called by ${team.name}`);
      }
    }
  }

  private handleSubstitutionEvent(event: SubstitutionEvent): void {
    if (this.idMatch === event.matchId) {
      const team = this.getTeamByPlayerId(event.playerOutId);
      if (team) {
        const playerOut = this.findPlayerById(event.playerOutId);
        const playerIn = this.findPlayerById(event.playerInId);
        if (playerOut && playerIn) {
          playerOut.onCourt = false;
          playerIn.onCourt = true;
          this.addRecentEvent('substitution',
            `Substitution: ${playerIn.name} IN, ${playerOut.name} OUT`);
        }
      }
    }
  }

  private handleQuarterChangeEvent(event: QuarterChangeEvent): void {
    if (this.idMatch === event.matchId) {
      this.quarter = event.quarter;
      this.time = 600; // Reset to 10 minutes
      this.matchStatus = 'in_progress';
      this.addRecentEvent('substitution', `Quarter ${this.quarter} starts`);
    }
  }

  private handleChronoEvent(event: ChronoEvent): void {
    if (this.idMatch === event.matchId) {
      this.time = this.parseTime(event.time);
      this.isRunning = event.isRunning;

      if (event.isRunning) {
        this.startTimer(true);  // Pass a flag to trigger immediate update
      } else {
        this.stopTimer();
      }
    }
  }


  private startTimer(immediate: boolean = false): void {
    if (!this.timerInterval && this.isRunning) {
      if (immediate && this.time > 0 && !this.isTimeout) {
        this.time--; // Mettez à jour immédiatement l'horloge
      }

      this.timerInterval = setInterval(() => {
        if (this.time > 0 && !this.isTimeout) {
          this.time--;
        } else if (this.isTimeout && this.timeoutTime > 0) {
          this.timeoutTime--;
          if (this.timeoutTime === 0) {
            this.isTimeout = false;
            this.matchStatus = 'in_progress';
          }
        } else if (this.time <= 0) {
          clearInterval(this.timerInterval);
          this.timerInterval = null;
        }
      }, 1000);
    }
  }



  private stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

  // Utility Methods
  private findPlayerById(playerId: number | undefined): PlayerMatch | null {
    if (playerId === undefined) return null;
    return (
      this.team1.players.find(p => p.id === playerId) ||
      this.team2.players.find(p => p.id === playerId) ||
      null
    );
  }

  private getTeamByPlayerId(playerId: number | undefined): Team | null {
    if (playerId === undefined) return null;
    if (this.team1.players.some(p => p.id === playerId)) return this.team1;
    if (this.team2.players.some(p => p.id === playerId)) return this.team2;
    return null;
  }

  private parseTime(timeString: string): number {
    const [hours, minutes, seconds] = timeString.split(':').map(Number);
    return hours * 3600 + minutes * 60 + seconds;
  }

  private addRecentEvent(type: 'score' | 'foul' | 'timeout' | 'substitution', description: string): void {
    const minutes = Math.floor(this.time / 60);
    const seconds = this.time % 60;
    const timeStr = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

    this.recentEvents.unshift({
      time: timeStr,
      description,
      type
    });

    // Keep only last 10 events
    if (this.recentEvents.length > 10) {
      this.recentEvents.pop();
    }
  }

  // Public Methods for Template
  getOnCourtPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => p.onCourt);
  }

  getBenchPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => !p.onCourt);
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  getMatchStatusDisplay(): string {
    switch (this.matchStatus) {
      case 'not_started': return 'Match Starting Soon';
      case 'in_progress': return 'Live';
      case 'timeout': return 'Timeout';
      case 'quarter_break': return 'Quarter Break';
      case 'finished': return 'Match Finished';
      default: return '';
    }
  }
}
