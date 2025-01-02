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
  matchDateTime: Date | null = null;

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
        if (data.matchDate) {
          this.matchDateTime = new Date(data.matchDate);
        }

        console.log('Team 1 players after load:', this.team1.players);  // Debug log
        console.log('Team 2 players after load:', this.team2.players);  // Debug log

        // Load events AFTER we have player information
        this.loadRecentEvents();
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
      if (statusUpdate.matchId === this.idMatch) {
        const oldStatus = this.matchStatus;
        this.matchStatus = statusUpdate.matchStatus;
        if (oldStatus === 'NotStarted' && statusUpdate.matchStatus === 'Live') {
          // Match just started
          this.addRecentEvent('status', 'Match started');
          this.loadMatchDetails(this.idMatch);
        } 
        if (this.matchStatus === 'Live') {
          this.loadMatchDetails(this.idMatch); //si le statut du match était à NotStarted et qu'il passe à live on reload la pages
        }
        if (this.matchStatus === 'Finished') {
          this.addRecentEvent('status', `Match ended - Final Score: ${this.team1.name} ${this.team1.score} - ${this.team2.score} ${this.team2.name}`);
          this.isRunning = false;
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
        this.addRecentEvent('score', `${player.name} (#${player.number}) scores ${event.points} points`);
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
      this.quarter = event.quarter+1; //car dans le backc ommecne a 0 pas 1
      this.time = this.quarterTime;
      this.isRunning = false;
      this.addRecentEvent('quarter', `Quarter ${event.quarter+1} starts`);
    }
  }

  private handleChronoEvent(event: ChronoEvent): void {
    if (this.idMatch === event.matchId) {
      this.isRunning = event.isRunning;
      this.time = this.parseTime(event.time);
      this.addRecentEvent('chrono', `Clock is ${event.isRunning ? 'running' : 'stopped'}`);
    }
  }

  private endTimeout(): void {
    this.isTimeout = false;
    this.timeoutTime = 0;
  }

  // Utility methods remain the same
  private findPlayerById(playerId: number | undefined): PlayerMatch | null {
    console.log('Finding player by id:', playerId);  // Log 10
    console.log('Team 1 players:', this.team1.players);  // Log 11
    console.log('Team 2 players:', this.team2.players);
    console.log('Found player:', this.team1.players.find(p => p.id === playerId) ||
      this.team2.players.find(p => p.id === playerId) ||
      null);
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

  private addRecentEvent(type:'score' | 'foul' | 'timeout' | 'substitution' | 'quarter'|'chrono'|'status', description: string): void {
    const timeStr = this.formatTime(this.time);
    const event = {
      icon: this.getEventIcon(type),
      quarter: this.quarter,  
      time: timeStr,
      description: description,
      type
    };
    this.recentEvents.unshift(event);
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
  public getEventIcon(eventType: string): string {
    switch (eventType.toLowerCase()) {
      case 'score':
      case 'basket':
        return '🏀';
      case 'foul':
        return '⚠️';
      case 'substitution':
        return '🔄';
      case 'timeout':
        return '⏳';
      case 'quarter':
        return '⏰';
      case 'chrono':
        return '⏱️';
      case 'status':
        return '📢';
      default:
        console.warn('Unknown event type for icon:', eventType);
        return '❓';
    }
  }
  private loadRecentEvents(): void {
    this.matchService.getMatchEvents(this.idMatch).subscribe({
      next: (events) => {
        this.recentEvents = events.map(event => {
          console.log('Processing event:', event);  
          // Normalize the event type
          let normalizedType = event.eventType.toLowerCase();
          if (normalizedType === 'basket') normalizedType = 'score';
          if (normalizedType === 'quarterchange') normalizedType = 'quarter'; 
          console.log('Formatted description:', this.formatEventDescription(event));
          return {
            icon: this.getEventIcon(normalizedType),
            quarter: event.quarter,
            time: event.time,
            description: this.formatEventDescription(event),
            type: normalizedType
          };
          console.log('Final processed events:', this.recentEvents);
        });
        console.log('Recent events loaded:', this.recentEvents);
      },
      error: (err) => console.error('Error fetching recent events:', err)
    });
  }

  private formatEventDescription(event: any): string {
    const type = event.eventType?.toLowerCase() || '';
    const getPlayerName = (id: number) => {
      const currentPlayer = this.findPlayerById(id);
      if (currentPlayer) {
        return `${currentPlayer.name} (#${currentPlayer.number})`;
      }
      // If the event contains player data (for loaded events)
      if (event.player) {
        console.log('Player info from event:', event.player);
        return `${event.playerName} (#${event.playerNumber})`;
      }
      console.log('No player information found for id:', id);
      // Fallback
      return `Player ${id}`;
    };

    switch (type) {
      case 'basket':
      case 'score':
        return `${getPlayerName(event.playerId)} scores ${event.points} points`;
      case 'foul':
        return `${getPlayerName(event.playerId)} committed a foul (${event.foulType})`;
      case 'substitution':
        return `Substitution: ${getPlayerName(event.playerInId)} IN, ${getPlayerName(event.playerOutId)} OUT`;
      case 'timeout':
        return `Timeout called by ${event.team}`;
      case 'quarter':
      case 'quarterchange':  // Changed from 'quarter' to match the event type
        return `Quarter ${event.quarter} starts`;
      case 'chrono':
        return `Clock is ${event.isRunning ? 'running' : 'stopped'}`;
      default:
        console.warn('Unknown event type:', type, event);
        return 'Unknown event.';
    }
  }


}
