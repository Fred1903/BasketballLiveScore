import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private apiUrl = 'https://localhost:7271/api/Players';
  constructor(private http: HttpClient) { }

  createPlayer(player: any): Observable<any> {
    return this.http.post(this.apiUrl + "/create", player);
  }

  getPositions(): Observable<{ value: string; display: string }[]> {
    return this.http.get<{ value: string; display: string }[]>(`${this.apiUrl}/positions`);
  }
}

