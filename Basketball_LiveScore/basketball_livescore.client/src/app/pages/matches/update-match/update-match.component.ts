import { Component, OnInit, OnDestroy } from '@angular/core';
import {
  PlayerMatch, Team, MatchState, SubstitutionEvent, ScoreEvent, FoulEvent,
  TimeoutEvent, MatchDetails,
  BasketEvent,
  QuarterChangeEvent,
  ChronoEvent
} from '../../../models/interfaces';
import { MatchSettingsService } from '../../../services/match-settings.service'
import { MatchService } from '../../../services/match.service'

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
  foulTypes: { id: number; name: string }[] = [];
  basketPoints: { id: number; name: number }[] = [];
  selectedPoint: string | number = '';
  selectedFoul: string | number = '';
  matchDetails: MatchDetails | null = null;
  idMatch = 2;
  private isLocalUpdate: boolean = false;


  team1: Team = {
    name: '',
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

  constructor(private matchSettingsService: MatchSettingsService, private matchService: MatchService) { }

  ngOnInit() {
    this.startTimer();

    this.loadFoulTypes();
    this.loadBasketPoints();
    this.loadMatchDetails(this.idMatch); //en paramètre on met l'id du match qu'on veut recup
    this.matchService.joinMatchGroup(this.idMatch); //D'abord on rejoint le groupe signalR qui fait les notifs depuis le hub

    //Souscriptions : si event par signalR, màj du front
    this.matchService.subscribeToBasketEvents((eventData) => {
      if (eventData.points !== undefined && eventData.playerId !== undefined) {
        this.updateScores(eventData.matchId, eventData.points, eventData.playerId);
      } else {
        console.error('Invalid event data:', eventData);
      }
    });
    this.matchService.subscribeToFoulEvents((eventData) => {
      this.handleFoulEvent(eventData);
    });

    this.matchService.subscribeToSubstitutionEvents((eventData) => {
      this.handleSubstitutionEvent(eventData);
    });

    this.matchService.subscribeToTimeoutEvents((eventData) => {
      this.handleTimeoutEvent(eventData);
    });

    this.matchService.subscribeToQuarterChangeEvents((eventData) => {
      this.handleQuarterChangeEvent(eventData);
    });

    this.matchService.subscribeToChronoEvents((eventData) => {
      this.handleChronoEvent(eventData);
    });

  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }


  loadMatchDetails(matchId: number): void {
    this.matchSettingsService.getMatchDetails(matchId).subscribe({
      next: (data) => {
        this.matchDetails = data;
        // Met à jour les équipes et les joueurs
        this.team1 = {
          name: data.homeTeam.name,
          score: data.scoreHome,
          timeouts: data.timeouts || 4,
          players: data.players
            .filter((p) => p.isHomeTeam)
            .map((player) => ({
              id: player.playerId,
              number: player.playerNumber,
              name: player.playerName,
              fouls: 0, // Les fautes peuvent être ajoutées ici
              points: 0, // Initialisé à 0, ou mets les points réels si disponibles
              onCourt: player.isStarter,
            })),
          coach: data.homeTeam.coach,
        };
        this.team2 = {
          name: data.awayTeam.name,
          score: data.scoreAway,
          timeouts: data.timeouts || 4,
          players: data.players
            .filter((p) => !p.isHomeTeam)
            .map((player) => ({
              id: player.playerId,
              number: player.playerNumber,
              name: player.playerName,
              fouls: 0,
              points: 0,
              onCourt: player.isStarter,
            })),
          coach: data.awayTeam.coach,
        };
        
      },
      error: (err) => {
        console.error('Error loading match details:', err);
      },
    });
  }


  loadFoulTypes(): void {
    this.matchSettingsService.getFoulTypes().subscribe({
      next: (data) => {
        this.foulTypes = data.map(foul => ({
          id: foul.id,
          name: foul.name
        }));
      },
      error: (err) => {
        console.error('Error loading foul types:', err);
      }
    });
  }

  loadBasketPoints(): void {
    this.matchSettingsService.getBasketPoints().subscribe({
      next: (data) => {
        this.basketPoints = data.map(point => ({
          id: point.id,
          name: point.id
        }));
      },
      error: (err) => {
        console.error('Error loading basket points:', err);
      }
    });
  }

  scrollToBench(team: 'team1' | 'team2'): void {
    setTimeout(() => {//on met un timeout pck sinon le dom n a pas encore chargé et ca n ira pas en bas
      const benchElement = document.getElementById(`${team}-bench`);
      if (benchElement) {
        benchElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }, 100); 
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
    if (this.isTimeout) {
      return;
    }

    this.isLocalUpdate = true;
    this.isRunning = !this.isRunning;

    const minutes = Math.floor(this.time / 60);
    const seconds = this.time % 60;
    const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

    const chronoEvent: ChronoEvent = {
      matchId: this.idMatch,
      quarter: this.quarter,
      time: formattedTime,
      isRunning: this.isRunning,
    };

    this.matchService.addChronoEvent(chronoEvent).subscribe({
      next: () => {
        this.isLocalUpdate = false;
      },
      error: (err) => {
        console.error('Error updating chrono:', err);
        this.isLocalUpdate = false;
      }
    });
  }

  nextQuarter() {
    if (!this.isRunning || this.time === 0) {
      return;
    }
    if (this.quarter < 4) {
      this.isLocalUpdate = true;

      // Get current formatted time
      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

      // Create quarter change event
      const quarterChangeEvent: QuarterChangeEvent = {
        matchId: this.idMatch,
        quarter: this.quarter + 1,  // Send the next quarter number
        time: formattedTime
      };

      this.matchService.addQuarterChangeEvent(quarterChangeEvent).subscribe({
        next: () => {
          // Update local state after successful server update
          this.quarter++;
          this.time = 600;
          this.isRunning = false;
          this.isLocalUpdate = false;
        },
        error: (err) => {
          console.error('Error updating quarter:', err);
          this.isLocalUpdate = false;
        }
      });
    }
  }
  handleQuarterChangeEvent(event: QuarterChangeEvent): void {
    if (this.idMatch === event.matchId && !this.isLocalUpdate) {
      this.quarter = event.quarter;
      this.time = 600;  // Reset time for new quarter
      this.isRunning = false;
    }
  }

  handleScoreChange(team: Team, player: PlayerMatch, event: any) {
    if (!this.isRunning || this.time === 0) {
      return;
    }
    const points = +event.target.value;
    if (points) {
      team.score += points;
      player.points += points;

      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`; // Format hh:mm:ss

      // On créé le basketEvent pr ensuite le notifier avec signalR
      const basketEvent: BasketEvent = {
        matchId: this.idMatch,
        playerId: player.id,
        points: points,
        quarter: this.quarter,
        time: formattedTime,
      };
      this.isLocalUpdate = true; //c est un update en local donc on ne veut pas que signalR nous re update
      this.matchService.addBasketEvent(basketEvent).subscribe({
        next: () => {
          this.isLocalUpdate = false;
        },
        error: (err) => {
          this.isLocalUpdate = false;
        },
      });

      this.selectedPoint = ''; // Réinitialise le select
      event.target.value = '';
    }
  }
  updateScores(matchId: number, points: number, playerId: number): void {
    console.log("Dans update score...") //on ne veut pas update 2 fois le score (1 fois via front et 1 fois signalR)
    if (this.idMatch === matchId && !this.isLocalUpdate) {
      // Met à jour le score de l'équipe
      const player = this.team1.players.find(p => p.id === playerId) ||
        this.team2.players.find(p => p.id === playerId);
      if (player) {
        player.points += points;

        // Met à jour le score de l'équipe
        if (this.team1.players.includes(player)) {
          this.team1.score += points;
        } else {
          this.team2.score += points;
        }
      }
    }
  }

  handleFoul(player: PlayerMatch, event: any) {
    if (!this.isRunning || this.time === 0) {
      return;
    }
    const foulTypeId = +event.target.value;
    if (foulTypeId >= 0) {
      // Local update
      this.isLocalUpdate = true;
      player.fouls++;

      // Create foul event
      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

      const foulEvent: FoulEvent = {
        matchId: this.idMatch,
        playerId: player.id,
        teamId: this.team1.players.includes(player) ? 1 : 2, // Or however you track team IDs
        quarter: this.quarter,
        time: formattedTime,
        timestamp: new Date(),
        foulType: this.foulTypes.find(f => f.id === foulTypeId)?.name || 'Unknown'
      };

      // Send to server via SignalR
      this.matchService.addFoulEvent(foulEvent).subscribe({
        next: () => {
          this.isLocalUpdate = false;
          this.selectedFoul = '';
          event.target.value = '';
        },
        error: (err) => {
          console.error('Error adding foul:', err);
          this.isLocalUpdate = false;
        }
      });
    }
  }
  handleFoulEvent(event: FoulEvent): void {
    if (this.idMatch === event.matchId && !this.isLocalUpdate) {
      const player = this.getPlayerById(event.playerId);
      if (player) {
        player.fouls++;
      }
    }
  }


  startTimeout(team: Team) {
    if (!this.isRunning || this.time === 0) {
      return;
    }
    if (team.timeouts > 0) {
      this.isLocalUpdate = true;

      // Update local state
      this.isRunning = false;
      this.isTimeout = true;
      this.timeoutTime = 30;
      team.timeouts--;

      // Create timeout event
      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

      const timeoutEvent: TimeoutEvent = {
        matchId: this.idMatch,
        team: team === this.team1 ? 'Home' : 'Away',
        teamId: team === this.team1 ? 1 : 2,
        quarter: this.quarter,
        time: formattedTime,
        timestamp: new Date()
      };

      this.matchService.addTimeoutEvent(timeoutEvent).subscribe({
        next: () => {
          this.isLocalUpdate = false;
        },
        error: (err) => {
          console.error('Error adding timeout:', err);
          this.isLocalUpdate = false;
        }
      });
    }
  }
  handleTimeoutEvent(event: TimeoutEvent): void {
    if (this.idMatch === event.matchId && !this.isLocalUpdate) {
      const team = event.team === 'Home' ? this.team1 : this.team2;
      if (team && team.timeouts > 0) {
        team.timeouts--;
        this.isRunning = false;
        this.isTimeout = true;
        this.timeoutTime = 30;
      }
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
    //quaond on appuie sur faire un changement, on veut que ca nous envoie directement le banc pour select le joueur
    const teamKey = team === this.team1 ? 'team1' : 'team2';
    this.scrollToBench(teamKey)
  }

  scrollToTop(): void {//si on cancel le changement, on reva au haut de la page
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }


  /*completeSubstitution(playerIn: PlayerMatch): void {
    if (this.activeTeam && this.playerToSubOut) {
      this.activeTeam.players = this.activeTeam.players.map(p => ({
        ...p,
        onCourt: p.id === this.playerToSubOut?.id ? false :
          p.id === playerIn.id ? true :
            p.onCourt
      }));
    }
    this.cancelSubstitution();
    setTimeout(() => {//apres un changement on reva vers le haut de la page
      this.scrollToTop();
    }, 100); 
  }*/

  completeSubstitution(playerIn: PlayerMatch): void {
    if (this.activeTeam && this.playerToSubOut) {
      this.isLocalUpdate = true;

      // Create substitution event
      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

      const substitutionEvent: SubstitutionEvent = {
        matchId: this.idMatch,
        playerInId: playerIn.id,
        playerOutId: this.playerToSubOut.id,
        teamId: this.activeTeam === this.team1 ? 1 : 2,
        quarter: this.quarter,
        time: formattedTime,
        timestamp: new Date()
      };

      this.matchService.addSubstitutionEvent(substitutionEvent).subscribe({
        next: () => {
          // Update local state after successful server update
          this.activeTeam!.players = this.activeTeam!.players.map(p => ({
            ...p,
            onCourt: p.id === this.playerToSubOut?.id ? false :
              p.id === playerIn.id ? true :
                p.onCourt
          }));
          this.isLocalUpdate = false;
          this.cancelSubstitution();
          this.scrollToTop();
        },
        error: (err) => {
          console.error('Error adding substitution:', err);
          this.isLocalUpdate = false;
        }
      });
    }
  }

  cancelSubstitution(): void {
    this.substitutionActive = false;
    this.activeTeam = null;
    this.playerToSubOut = null;
    this.scrollToTop();
  }

  handleSubstitution(team: Team, playerIn: PlayerMatch) {
    if (this.selectedPlayerForSub) {
      this.isLocalUpdate = true;

      // Update local state
      team.players = team.players.map(p => ({
        ...p,
        onCourt: p.id === this.selectedPlayerForSub?.id ? false :
          p.id === playerIn.id ? true :
            p.onCourt
      }));

      // Create substitution event
      const minutes = Math.floor(this.time / 60);
      const seconds = this.time % 60;
      const formattedTime = `00:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

      const substitutionEvent: SubstitutionEvent = {
        matchId: this.idMatch,
        playerInId: playerIn.id,
        playerOutId: this.selectedPlayerForSub.id,
        teamId: team === this.team1 ? 1 : 2,
        quarter: this.quarter,
        time: formattedTime,
        timestamp: new Date()
      };

      this.matchService.addSubstitutionEvent(substitutionEvent).subscribe({
        next: () => {
          this.isLocalUpdate = false;
          this.selectedPlayerForSub = null;
        },
        error: (err) => {
          console.error('Error adding substitution:', err);
          this.isLocalUpdate = false;
        }
      });
    }
  }

  handleSubstitutionEvent(event: SubstitutionEvent): void {
    if (this.idMatch === event.matchId && !this.isLocalUpdate) {
      const team = this.getTeamByPlayerId(event.playerOutId);
      if (team) {
        team.players = team.players.map(p => ({
          ...p,
          onCourt: p.id === event.playerOutId ? false :
            p.id === event.playerInId ? true :
              p.onCourt
        }));
      }
    }
  }
  getTeamByPlayerId(playerId: number): Team | null {
    if (this.team1.players.some(p => p.id === playerId)) return this.team1;
    if (this.team2.players.some(p => p.id === playerId)) return this.team2;
    return null;
  }
  getOnCourtPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => p.onCourt);
  }

  getBenchPlayers(team: Team): PlayerMatch[] {
    return team.players.filter(p => !p.onCourt);
  }

  

  //Tous les event qui recoivent avec signalR sauf pour les points c fait dans la meme methode


  /*handleSubstitutionEvent(event: SubstitutionEvent): void {
    const playerOut = this.getPlayerById(event.playerOutId);
    const playerIn = this.getPlayerById(event.playerInId);
    if (playerOut && playerIn) {
      playerOut.onCourt = false;
      playerIn.onCourt = true;
      console.log(`Player ${playerOut.name} substituted with ${playerIn.name}`);
    }
  }*/


  /*handleQuarterChangeEvent(event: QuarterChangeEvent): void {
    this.quarter = event.quarter;
    console.log(`Quarter changed to ${this.quarter}`);
  }*/

  /*handleChronoEvent(event: ChronoEvent): void {
    this.isRunning = event.isRunning;
    this.time = this.parseTime(event.time); // Fonction pour convertir le temps en secondes
    console.log(`Chrono is now ${this.isRunning ? 'running' : 'stopped'}`);
  }*/

  handleChronoEvent(event: ChronoEvent): void {
    if (this.idMatch === event.matchId && !this.isLocalUpdate) {
      this.isRunning = event.isRunning;
      this.time = this.parseTime(event.time);
    }
  }

  // Utilitaires pour trouver un joueur
  getPlayerById(playerId: number | undefined): PlayerMatch | null {
    if (!playerId) return null;
    return (
      this.team1.players.find((p) => p.id === playerId) ||
      this.team2.players.find((p) => p.id === playerId) ||
      null
    );
  }

  // Utilitaire pour parser le temps
  parseTime(timeString: string): number {
    const [hours, minutes, seconds] = timeString.split(':').map(Number);
    return hours * 3600 + minutes * 60 + seconds;
  }

}
