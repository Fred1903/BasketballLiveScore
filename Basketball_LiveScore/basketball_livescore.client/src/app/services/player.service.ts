import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, throwError } from 'rxjs';
import { Player, Position } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private apiUrl = 'https://localhost:7271/api/Players';
  constructor(private http: HttpClient) { }
  
  createPlayer(player: Player): Observable<any> {
    return this.http.post(this.apiUrl + "/create", player).pipe(
      catchError((error) => {//Si il y a déjà une player avec ce numero, le back renvoie une erreur  
        return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
      })
    );
  }

  getPositions(): Observable<Position[]> {
    return this.http.get<Position[]>(`${this.apiUrl}/positions`);
  }

}

