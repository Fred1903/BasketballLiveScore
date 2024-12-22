import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class MatchService {
  private apiUrl = 'https://localhost:7271/api/Match'; 

  constructor(private http: HttpClient) { }

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
