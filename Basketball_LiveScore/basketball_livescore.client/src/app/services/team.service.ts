  import { Injectable } from '@angular/core';
  import { HttpClient } from '@angular/common/http';
  import { Observable, throwError } from 'rxjs';
  import { catchError, map } from 'rxjs/operators';
  import { Team } from '../models/interfaces';


  @Injectable({
    providedIn: 'root',
  })
  export class TeamService {
    private apiUrl = 'https://localhost:7271/api/Teams';

    constructor(private http: HttpClient) { }


    createTeam(team: Team): Observable<any> {
      return this.http.post(`${this.apiUrl}/create`, team).pipe(
        catchError((error) => {//Si il y a déjà une équipe avec ce nom, le back renvoie une erreur  
          return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
        })
      );
    }


    getTeams(): Observable<any[]> {
      return this.http.get<any[]>(`${this.apiUrl}/all`);
    }

    getPlayersByTeam(teamId: number): Observable<any[]> {
      return this.http.get<any>(`${this.apiUrl}/${teamId}`).pipe(
        map((team) => {
          if (!team || !team.players) {
            throw new Error('No players found for this team.');
          }
          return team.players.map((player: any) => ({
            id: player.id,
            firstName: player.firstName,
            lastName: player.lastName,
            number: player.number,
            position: player.position,
          }));
        })
      );
    }

  }
