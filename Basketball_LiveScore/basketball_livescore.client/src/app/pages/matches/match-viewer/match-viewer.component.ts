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
        this.loadRecentEvents();
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
    const icon = this.getEventIcon(type);
    const timeStr = this.formatTime(this.time);
    this.recentEvents.unshift({ icon, time: timeStr, description, type });
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
  private getEventIcon(eventType: string): string {
    switch (eventType) {
      case 'Basket': return '🏀';
      case 'Foul': return '⚠️';
      case 'Substitution': return '🔄';
      case 'Timeout': return '⏳';
      case 'QuarterChange': return '⏰';
      case 'Chrono': return '⏱️';
      default: return '❓';
    }
  }
  private loadRecentEvents(): void {
    this.matchService.getMatchEvents(this.idMatch).subscribe({
      next: (events) => {
        this.recentEvents = events.map(event => ({
          icon: this.getEventIcon(event.eventType),
          time: event.time,
          description: this.formatEventDescription(event),
          type: event.eventType
        }));
        console.log('Recent events loaded:', this.recentEvents);
      },
      error: (err) => console.error('Error fetching recent events:', err)
    });
  }

  private formatEventDescription(event: any): string {
    switch (event.eventType) {
      case 'Basket':
        return `Player ${event.playerId} scored ${event.points} points.`;
      case 'Foul':
        return `Player ${event.playerId} committed a foul (${event.foulType}).`;
      case 'Substitution':
        return `Player ${event.playerInId} IN, Player ${event.playerOutId} OUT.`;
      case 'Timeout':
        return `Timeout called by ${event.team}.`;
      case 'QuarterChange':
        return `Quarter ${event.quarter} starts.`;
      case 'Chrono':
        return `Clock is ${event.isRunning ? 'running' : 'stopped'}.`;
      default:
        return 'Unknown event.';
    }
  }


}
