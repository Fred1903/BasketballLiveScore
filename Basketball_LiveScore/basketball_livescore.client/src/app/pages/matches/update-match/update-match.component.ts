import { Component, OnInit, OnDestroy } from '@angular/core';
import {
  PlayerMatch, Team, MatchState, SubstitutionEvent, ScoreEvent, FoulEvent,
  TimeoutEvent, MatchDetails,
  BasketEvent
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

  constructor(private matchSettingsService: MatchSettingsService, private matchService: MatchService) { }

  ngOnInit() {
    this.startTimer();

    this.loadFoulTypes();
    this.loadBasketPoints();
    this.loadMatchDetails(this.idMatch); //en paramètre on met l'id du match qu'on veut recup
    this.matchService.joinMatchGroup(this.idMatch); //D'abord on rejoint le groupe signalR qui fait les notifs depuis le hub
    /*this.matchService.subscribeToBasketEvents((eventData) => {//si event de basket par signalR, màj du front
      console.log("un event a été trigger avec signalR")
     //this.updateScores(eventData.matchId, eventData.scoreHome, eventData.scoreAway);
    });*/

    this.matchService.subscribeToBasketEvents((eventData) => {
      console.log('Basket event received:', eventData);
      if (eventData.points !== undefined && eventData.playerId !== undefined) {
        this.updateScores(eventData.matchId, eventData.points, eventData.playerId);
      } else {
        console.error('Invalid event data:', eventData);
      }

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
          timeouts: data.timeouts,
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

        console.log("team 1 : " + this.team1.name);
        this.team2 = {
          name: data.awayTeam.name,
          score: data.scoreAway,
          timeouts: data.timeouts,
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
    this.isRunning = !this.isRunning;
  }

  nextQuarter() {
    if (this.quarter < 4) {
      this.quarter++;
      this.time = 600;
      this.isRunning = false;
    }
  }

  handleScoreChange(team: Team, player: PlayerMatch, event: any) {
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
      console.log('Sending basket event:', basketEvent);
      this.isLocalUpdate = true; //c est un update en local donc on ne veut pas que signalR nous re update
      this.matchService.addBasketEvent(basketEvent).subscribe({
        next: () => {
          console.log('Basket event successfully added.');
          this.isLocalUpdate = false;
        },
        error: (err) => {
          console.error('Error adding basket event:', err);
          this.isLocalUpdate = false;
        },
      });

      this.selectedPoint = ''; // Réinitialise le select
      event.target.value = '';
    }
  }



  handleFoul(player: PlayerMatch, event: any) {
    const foulTypeId = +event.target.value;
    if (foulTypeId>=0) { //On veut aussi inclure l'id 0 
      player.fouls++;
      this.selectedFoul = '';  
      event.target.value = '';
    }
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
    //quaond on appuie sur faire un changement, on veut que ca nous envoie directement le banc pour select le joueur
    const teamKey = team === this.team1 ? 'team1' : 'team2';
    this.scrollToBench(teamKey)
  }

  scrollToTop(): void {//si on cancel le changement, on reva au haut de la page
    window.scrollTo({ top: 0, behavior: 'smooth' });
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
    setTimeout(() => {//apres un changement on reva vers le haut de la page
      this.scrollToTop();
    }, 100); 
  }

  cancelSubstitution(): void {
    this.substitutionActive = false;
    this.activeTeam = null;
    this.playerToSubOut = null;
    this.scrollToTop();
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


  /*updateScores(matchId: number, scoreHome: number, scoreAway: number): void {
    if (this.idMatch === matchId) {
      this.team1.score = scoreHome; // Mettez à jour le score de l'équipe 1
      this.team2.score = scoreAway; // Mettez à jour le score de l'équipe 2
    }
  }*/

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


  /*listenToSignalR(): void {
    this.matchService.hubConnection.on("BasketEventOccurred", (eventData) => {
      console.log("Basket event received:", eventData);
      this.updateScores(eventData.matchId, eventData.scoreHome, eventData.scoreAway);
    });
  }*/

}
