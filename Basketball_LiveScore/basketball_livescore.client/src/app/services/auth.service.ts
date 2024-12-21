// auth.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { UserLogin } from '../models/interfaces';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  // Observable for authentication status
  isAuthenticated$: Observable<boolean> = this.isAuthenticatedSubject.asObservable();

  private apiUrl = 'https://localhost:7271/api/Auth';
  constructor(private http: HttpClient) { }


  login(userLogin: UserLogin): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, userLogin).pipe(
      tap((response: any) => {
        // Assuming the API returns a JWT token in the response
        const token = response.token;
        if (token) {
          localStorage.setItem('authToken', token); // Store the token in local storage
          this.isAuthenticatedSubject.next(true); // Update authentication state
        }
      }),
      catchError((error: HttpErrorResponse) => {
        return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
      })
    );
  }


  logout(): void {
    // Reset the authentication state
    this.isAuthenticatedSubject.next(false);
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }
}
