import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { BasketEvent } from '../models/interfaces';

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
      .then(() => console.log('SignalR connection started'))
      .catch(err => console.error('Error starting SignalR connection:', err));
  }
  //pck hubConnection est en private donc pas acces depuis le component donc on met ca :
  subscribeToBasketEvents(callback: (eventData: BasketEvent) => void): void {
    this.hubConnection.on("BasketEventOccurred", (eventData: BasketEvent) => {
      callback(eventData);
    });
  }

  joinMatchGroup(matchId: number): void {
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


  addBasketEvent(event: BasketEvent): Observable<any> {
    return this.http.post(`${this.apiUrl}/${event.matchId}/add-basket`, event);
  }


  createMatch(payload: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, payload);
  }

  getDefaultSettings(): Observable<{ NumberOfQuarters: number; QuarterDuration: number; TimeoutDuration: number }> {
    return this.http.get<{ NumberOfQuarters: number; QuarterDuration: number; TimeoutDuration: number }>(
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
}
