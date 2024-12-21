import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Player, Position } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private apiUrl = 'https://localhost:7271/api/Players';
  constructor(private http: HttpClient) { }
  
  createPlayer(player: Player): Observable<any> {
    return this.http.post(this.apiUrl + "/create", player);
  }

  getPositions(): Observable<Position[]> {
    return this.http.get<Position[]>(`${this.apiUrl}/positions`);
  }
}

