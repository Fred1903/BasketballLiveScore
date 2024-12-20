import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';


@Injectable({
  providedIn: 'root',
})
export class TeamService {
  private apiUrl = 'https://localhost:7271/api/Teams';

  constructor(private http: HttpClient) { }


  createTeam(team: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, team).pipe(
      catchError((error) => {//Si il y a déjà une équipe avec ce nom, le back renvoie une erreur  
        return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
      })
    );
  }


  getTeams(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/all`);
  }
 
}
