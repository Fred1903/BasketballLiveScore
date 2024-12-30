/*import { Component, OnInit, OnDestroy } from '@angular/core';
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
  time: number = 0;
  isTimeout: boolean = false;
  timeoutTime: number = 0;
  matchDetails: MatchDetails | null = null;
  idMatch: number =0;
  recentEvents: RecentEvent[] = [];
  private intervalId: any;
  private timerInterval: any;
  isRunning: boolean = false;
  activeSubstitution: {
    playerIn: PlayerMatch | null;
    playerOut: PlayerMatch | null;
    team: Team | null;
    remaining: number;
  } | null = null;

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
    if (matchId) { //Que si le match existe on le load
      this.matchSettingsService.getMatchDetails(matchId).subscribe({
        next: (data) => {
          this.matchDetails = data;
          this.initializeTeams(data);
          this.determineMatchStatus(data);
        },
        error: (err) => console.error('Error loading match details:', err)
      });
    }
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
    if (this.idMatch && this.idMatch === event.matchId) {
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
    if (this.idMatch && this.idMatch === event.matchId) {
      const player = this.findPlayerById(event.playerId);
      if (player) {
        player.fouls++;
        this.addRecentEvent('foul', `Foul on ${player.name} (${event.foulType})`);
      }
    }
  }

  private handleTimeoutEvent(event: TimeoutEvent): void {
    if (this.idMatch && this.idMatch === event.matchId) {
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
    if (this.idMatch && this.idMatch === event.matchId) {
      const team = this.getTeamByPlayerId(event.playerOutId);
      if (team) {
        const playerOut = this.findPlayerById(event.playerOutId);
        const playerIn = this.findPlayerById(event.playerInId);
        if (playerOut && playerIn) {

          this.activeSubstitution = {
            playerIn,
            playerOut,
            team,
            remaining: 5
          };
          const countdownInterval = setInterval(() => {
            if (this.activeSubstitution) {
              this.activeSubstitution.remaining--;
              if (this.activeSubstitution.remaining <= 0) {
                clearInterval(countdownInterval);

                // Update player states
                playerOut.onCourt = false;
                playerIn.onCourt = true;

                // Add to recent events
                this.addRecentEvent('substitution',
                  `Substitution: ${playerIn.name} IN, ${playerOut.name} OUT`);

                this.activeSubstitution = null;
              }
            }
          }, 1000);
          setTimeout(() => {
            if (this.activeSubstitution) {
              playerOut.onCourt = false;
              playerIn.onCourt = true;
              this.activeSubstitution = null;
            }
          }, 5500);

          /*playerOut.onCourt = false;
          playerIn.onCourt = true;
          this.addRecentEvent('substitution',
            `Substitution: ${playerIn.name} IN, ${playerOut.name} OUT`); commment eeeeeeeeennnnnnnd/
        }
      }
    }
  }

  private handleQuarterChangeEvent(event: QuarterChangeEvent): void {
    if (this.idMatch && this.idMatch === event.matchId) {
      this.quarter = event.quarter;
      this.time = this.matchDetails?.quarterDuration * 60; 
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
}*/


import { Component, OnInit, OnDestroy, NgZone } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatchService } from '../../../services/match.service';
import { MatchSettingsService } from '../../../services/match-settings.service'
import {
  PlayerMatch, Team, MatchDetails, BasketEvent, FoulEvent,
  TimeoutEvent, SubstitutionEvent, QuarterChangeEvent, ChronoEvent, RecentEvent
} from '../../../models/interfaces';

@Component({
  selector: 'app-match-viewer',
  templateUrl: './match-viewer.component.html',
  styleUrls: ['./match-viewer.component.css']
})
export class MatchViewerComponent implements OnInit, OnDestroy {
  quarter: number = 0;
  time: number = 0;
  quarterTime: number = 0;
  isRunning: boolean = false;
  timeoutTime: number = 0;
  isTimeout: boolean = false;
  matchDetails: MatchDetails | null = null;
  idMatch: number = 0;
  recentEvents: RecentEvent[] = [];
  private timerInterval: any;
  timeoutDuration: number = 0;
  matchStatus: string = 'NotStarted';//Au debut n a pas encore commencé avant qu on init
  numberOfQuarters: number = 0;

  team1: Team = {
    name: '',
    score: 0,
    timeouts: 0,
    players: [],
    coach: ''
  };

  team2: Team = {
    name: '',
    score: 0,
    timeouts: 0,
    players: [],
    coach: ''
  };

  constructor(
    private matchService: MatchService,
    private route: ActivatedRoute,
    private matchSettingsService: MatchSettingsService,
    private ngZone: NgZone
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = parseInt(params['id']);
      if (!isNaN(id)) {
        this.idMatch = id;
        this.startTimer();
        this.loadMatchDetails(this.idMatch);
        this.matchService.joinMatchGroup(this.idMatch);
        this.subscribeToMatchEvents();
      }
    });
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  private loadMatchDetails(matchId: number): void {
    this.matchSettingsService.getMatchDetails(matchId).subscribe({
      next: (data) => {
        this.matchDetails = data;
        this.quarterTime = data.quarterDuration * 60;
        this.matchStatus = data.matchStatus;
        this.quarter = data.currentQuarter;
        this.time = data.currentTime;
        this.isRunning = data.isRunning;
        this.timeoutTime = data.timeoutDuration;
        this.timeoutDuration = data.timeoutDuration;
        this.numberOfQuarters = data.numberOfQuarters;

        this.team1 = {
          name: data.homeTeam.name,
          score: data.scoreHome,
          timeouts: data.homeTeamRemainingTimeouts,
          players: data.players
            .filter((p) => p.isHomeTeam)
            .map((player) => ({
              id: player.playerId,
              number: player.playerNumber,
              name: player.playerName,
              fouls: player.fouls,
              points: player.points,
              onCourt: player.onField,
            })),
          coach: data.homeTeam.coach,
        };

        this.team2 = {
          name: data.awayTeam.name,
          score: data.scoreAway,
          timeouts: data.awayTeamRemainingTimeouts,
          players: data.players
            .filter((p) => !p.isHomeTeam)
            .map((player) => ({
              id: player.playerId,
              number: player.playerNumber,
              name: player.playerName,
              fouls: player.fouls,
              points: player.points,
              onCourt: player.onField,
            })),
          coach: data.awayTeam.coach,
        };
        console.log("players team1 : " + this.team1.players);
      },
      error: (err) => console.error('Error loading match details:', err)
    });
  }

  /*private loadRecentEvents(matchId: number): void {
    this.matchService.getMatchEvents(matchId).subscribe(events => {
      this.recentEvents = events.map(event => ({
        time: this.formatTime(this.parseTime(event.time)),
        description: this.getEventDescription(event),
        type: event.type
      }));
    });
  }*/

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

  private subscribeToMatchEvents(): void {

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

    this.matchService.subscribeToMatchStatusEvents((statusUpdate) => {
      console.log("subscribeheinnnnnnnnnn")
      console.log("ssstautsUpdate.matchId = " + statusUpdate.matchId + " et this.idMatch = " + this.idMatch);
      if (statusUpdate.matchId === this.idMatch) {
        console.log("stautsUpdate.matchId = " + statusUpdate.matchId + " et this.idMatch = " + this.idMatch);
        console.log("statut = " + this.matchStatus);
        console.log("statuts.stauts = " + statusUpdate.matchStatus);
        this.matchStatus = statusUpdate.matchStatus;
        console.log("statute = " + this.matchStatus);
        console.log("statuts.stautes = " + statusUpdate.matchStatus);
        if (this.matchStatus === 'Live') {
          console.log("jeeeeeeeeeeeeee loaaaaad matchhhhhhhh")
          this.loadMatchDetails(this.idMatch); //si le statut du match était à NotStarted et qu'il passe à live on reload la pages
        }
      }
    });
  }

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
        this.isRunning = false;
        this.isTimeout = true;
        this.timeoutTime = this.timeoutDuration;
        this.addRecentEvent('timeout', `Timeout called by ${team.name}`);
      }
    }
  }

  private handleSubstitutionEvent(event: SubstitutionEvent): void {
    if (this.idMatch === event.matchId) {
      const team = this.getTeamByPlayerId(event.playerOutId);
      if (team) {
        team.players = team.players.map(p => ({
          ...p,
          onCourt: p.id === event.playerOutId ? false :
            p.id === event.playerInId ? true :
              p.onCourt
        }));

        const playerIn = this.findPlayerById(event.playerInId);
        const playerOut = this.findPlayerById(event.playerOutId);
        if (playerIn && playerOut) {
          this.addRecentEvent('substitution',
            `Substitution: ${playerIn.name} IN, ${playerOut.name} OUT`);
        }
      }
    }
  }

  private handleQuarterChangeEvent(event: QuarterChangeEvent): void {
    if (this.idMatch === event.matchId) {
      this.quarter = event.quarter;
      this.time = this.quarterTime;
      this.isRunning = false;
      this.addRecentEvent('quarter', `Quarter ${this.quarter} starts`);
    }
  }

  private handleChronoEvent(event: ChronoEvent): void {
    if (this.idMatch === event.matchId) {
      this.isRunning = event.isRunning;
      this.time = this.parseTime(event.time);
    }
  }

  private endTimeout(): void {
    this.isTimeout = false;
    this.timeoutTime = 0;
  }

  // Utility methods remain the same
  private findPlayerById(playerId: number | undefined): PlayerMatch | null {
    return (
      this.team1.players.find(p => p.id === playerId) ||
      this.team2.players.find(p => p.id === playerId) ||
      null
    );
  }

  private getTeamByPlayerId(playerId: number | undefined): Team | null {
    if (this.team1.players.some(p => p.id === playerId)) return this.team1;
    if (this.team2.players.some(p => p.id === playerId)) return this.team2;
    return null;
  }

  private parseTime(timeString: string): number {
    const [hours, minutes, seconds] = timeString.split(':').map(Number);
    return hours * 3600 + minutes * 60 + seconds;
  }

  private addRecentEvent(type: 'score' | 'foul' | 'timeout' | 'substitution' | 'quarter', description: string): void {
    const timeStr = this.formatTime(this.time);
    this.recentEvents.unshift({ time: timeStr, description, type });
    if (this.recentEvents.length > 10) {
      this.recentEvents.pop();
    }
  }

  // Public methods for template
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
}
