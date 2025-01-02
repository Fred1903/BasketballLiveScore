import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = 'https://localhost:7271/api/Users'; // Changez l'URL selon votre backend

  constructor(private http: HttpClient) { }

  getUsersByRole(role: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/role/${role}`);
  }

  addUserAsEncoder(userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/add-encoder/${userId}`, null).pipe(
      catchError((error) => {
        console.error("Error in addUserAsEncoder:", error);
        return throwError(() => new Error(error.error?.message || "An error occurred"));
      })
    );
  }

  removeUserFromEncoders(userId: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/remove-encoder/${userId}`, null).pipe(
      catchError((error) => {
        console.error("Error in removeUserFromEncoders:", error);
        return throwError(() => new Error(error.error?.message || "An error occurred"));
      })
    );
  }


}

