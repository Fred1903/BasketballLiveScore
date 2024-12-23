import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MatchDetails } from '../models/interfaces';

@Injectable({
  providedIn: 'root'
})
export class MatchSettingsService {
  private apiUrl = 'https://localhost:7271/api/Match'; 

  constructor(private http: HttpClient) { }

  getFoulTypes(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/foul-types`);
  }

  getBasketPoints(): Observable<{ id: number; name: string }[]> {
    return this.http.get<{ id: number; name: string }[]>(`${this.apiUrl}/basket-points`);
  }

  getMatchDetails(matchId: number): Observable<MatchDetails> {
    return this.http.get<MatchDetails>(`${this.apiUrl}/${matchId}`);
  }
}
