// auth.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { UserLogin, UserRegister } from '../models/interfaces';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  // Observable for authentication status
  isAuthenticated$: Observable<boolean> = this.isAuthenticatedSubject.asObservable();

  private apiUrl = 'https://localhost:7271/api/Auth';
  constructor(private http: HttpClient, private router: Router) { }


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

  register(userRegister: UserRegister): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, userRegister).pipe(
      catchError((error: HttpErrorResponse) => {
        return throwError(() => new Error(error.error?.message || 'An unknown error occurred.'));
      })
    );
  }

  getRole(): string | null {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1])); // Décoder le payload du JWT
        //la clé est celle qu'on a dans le back-end 'Claims.Role'
        return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null; 
      } catch (e) {
        console.error('Invalid token:', e);
        return null;
      }
    }
    return null;
  }

  getUserInfo(): any {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1])); // Décoder le payload du JWT
        return {
          id: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
          email: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
          firstName: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"],
          lastName: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"],
          username: payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
        };
      } catch (e) {
        console.error('Invalid token:', e);
        return null;
      }
    }
    return null; // Si aucun token n'est trouvé
  }


  logout(): void {
    localStorage.removeItem('authToken'); //avant de logout, on supprime le token !!!
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/matches']);//redirection vers page de match!!
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }
}
