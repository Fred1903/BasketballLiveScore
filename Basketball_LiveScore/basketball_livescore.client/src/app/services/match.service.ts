  import { Injectable } from '@angular/core';
  import { HttpClient } from '@angular/common/http';
  import { catchError, Observable, throwError } from 'rxjs';
  import * as signalR from '@microsoft/signalr';
  import { BasketEvent, ChronoEvent, FoulEvent, MatchLiveScore, QuarterChangeEvent, SubstitutionEvent, TimeoutEvent } from '../models/interfaces';

  @Injectable({
    providedIn: 'root',
  })
  export class MatchService {
    private apiUrl = 'https://localhost:7271/api/Match';
    private hubConnection: signalR.HubConnection;

    constructor(private http: HttpClient) {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7271/matchHub')
        .withAutomaticReconnect()
        .build();

      this.hubConnection //Log signalR 
        .start()
        .then(() => { console.log('SignalR connection started');
          this.hubConnection.onreconnecting(() => console.log('Reconnecting...'));
          this.hubConnection.onreconnected(() => console.log('Reconnected'));
          this.hubConnection.onclose(() => console.log('Connection closed'));
        })
        .catch(err => console.error('Error starting SignalR connection:', err));
    }
    //Ci-dessous les souscriptions : 
    //pck hubConnection est en private donc pas acces depuis le component donc on met ca :
    subscribeToBasketEvents(callback: (eventData: BasketEvent) => void): void {
      console.log('Subscribing to basket events');
      this.hubConnection.on("BasketEventOccurred", (eventData: BasketEvent) => {
        callback(eventData);
      });
    }
    subscribeToFoulEvents(callback: (eventData: FoulEvent) => void): void {
      this.hubConnection.on('FoulEventOccurred', callback);
    }

    subscribeToSubstitutionEvents(callback: (eventData: SubstitutionEvent) => void): void {
      this.hubConnection.on('SubstitutionEventOccurred', callback);
    }

    subscribeToTimeoutEvents(callback: (eventData: TimeoutEvent) => void): void {
      this.hubConnection.on('TimeoutEventOccurred', callback);
    }

    subscribeToQuarterChangeEvents(callback: (eventData: QuarterChangeEvent) => void): void {
      this.hubConnection.on('QuarterChangeEventOccurred', callback);
    }
    subscribeToQuarterUpdates(callback: (update: { matchId: number; currentQuarter: number }) => void): void {
      this.hubConnection.on("QuarterUpdate", callback);
    }


    subscribeToChronoEvents(callback: (eventData: ChronoEvent) => void): void {
      this.hubConnection.on('ChronoEventOccurred', callback);
    }

    subscribeToMatchStatusEvents(callback: (statusUpdate: { matchId: number; matchStatus: string }) => void): void {
      this.hubConnection.on('MatchStatusChanged', callback);
    }
    subscribeToTimerUpdates(callback: (timerUpdate: { matchId: number; currentQuarter: number; remainingTime: number }) => void): void {
      this.hubConnection.on('TimerUpdated', callback);
    }


    joinMatchGroup(matchId: number): void {//on join le signalR de ce match
      //connexion doit etre 'Connected' sinon fonctionnera ´pas 
      if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
        this.hubConnection
          .invoke('JoinMatchGroup', matchId.toString())
          .then(() => console.log(`Successfully joined SignalR group for match ${matchId}`))
          .catch(err => console.error(`Error joining SignalR group for match ${matchId}:`, err));
      } else {
        //On doit mettre un timeOut pck la connection ne se fait pas directement
        setTimeout(() => this.joinMatchGroup(matchId), 1000);
      }
    }

    //Ajouts des différents event via leurs API
    addBasketEvent(event: BasketEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-basket`, event);
    }
    addFoulEvent(event: FoulEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-foul`, event);
    }

    addSubstitutionEvent(event: SubstitutionEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-substitution`, event);
    }

    addTimeoutEvent(event: TimeoutEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-timeout`, event);
    }

    addQuarterChangeEvent(event: QuarterChangeEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-quarter-change`, event);
    }

    addChronoEvent(event: ChronoEvent): Observable<any> {
      return this.http.post(`${this.apiUrl}/${event.matchId}/add-chrono`, event);
    }


    //Creation match avec tt les données
    createMatch(payload: any): Observable<any> {
      return this.http.post(`${this.apiUrl}/create`, payload).pipe(
        catchError((error) => {//Gestion des erreurs du back-end  
          return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
        })
      );
    }

    startMatch(matchId: number): void {
      this.http.post(`${this.apiUrl}/start/${matchId}`, {}).subscribe({
        next: () => alert('Match started!'),
        error: (err) => console.error('Error starting match:', err)
      });
    }
    finishMatch(matchId: number): Observable<any> {
      return this.http.put(`${this.apiUrl}/finish/${matchId}`, {});
    }

    //GetW depuis le back les settings par défauts et options possiblesE
    getDefaultSettings(): Observable<{ NumberOfQuarters: number; QuarterDuration: number; TimeoutDuration: number; TimeoutAmount:number }> {
      return this.http.get<{ NumberOfQuarters: number; QuarterDuration: number; TimeoutDuration: number,TimeoutAmount:number }>(
        `${this.apiUrl}/settings/defaults`
      );
    }

    getNumberOfQuartersOptions(): Observable<number[]> {
      return this.http.get<number[]>(`${this.apiUrl}/settings/number-of-quarters`);
    }

    getQuarterDurationOptions(): Observable<number[]> {
      return this.http.get<number[]>(`${this.apiUrl}/settings/quarter-durations`);
    }

    getTimeoutDurationOptions(): Observable<number[]> {
      return this.http.get<number[]>(`${this.apiUrl}/settings/timeout-durations`);
    }

    getTimeoutAmountOptions(): Observable<number[]> {
      return this.http.get<number[]>(`${this.apiUrl}/settings/timeout-amount`);
    }

    getAllMatches(): Observable<MatchLiveScore[]> {
      return this.http.get<MatchLiveScore[]>(`${this.apiUrl}/all`);
    }
    getMatchEvents(matchId: number): Observable<any[]> {
      return this.http.get<any[]>(`${this.apiUrl}/events/${matchId}`);
    }

  }
