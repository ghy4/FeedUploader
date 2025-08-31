import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, catchError, of, map } from 'rxjs';

export interface AppUser {
  id: number;
  name: string;
  surname: string;
  email: string;
  contactNumber: string;
  role: string;
  status?: string;
  lastActivity?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'https://localhost:7281/api/User';
  constructor(private http: HttpClient) {}

  getUsers(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(`${this.apiUrl}`).pipe(
      catchError(err => {
        console.error('Failed to load users', err);
        return of([]);
      })
    );
  }

  getUser(id: number): Observable<AppUser | null> {
    return this.http.get<AppUser>(`${this.apiUrl}/${id}`).pipe(
      catchError(err => {
        if (err.status !== 404) console.error('Get user error', err);
        return of(null);
      })
    );
  }

  getUserByEmail(email: string): Observable<AppUser | null> {
    return this.http.get<AppUser>(`${this.apiUrl}/email/${encodeURIComponent(email)}`).pipe(
      catchError(err => {
        if (err.status !== 404) console.error('Get user by email error', err);
        return of(null);
      })
    );
  }

  createUser(payload: { name: string; surname: string; email: string; password: string; contactNumber: string; role: string; }): Observable<AppUser | null> {
    return this.http.post<AppUser>(`${this.apiUrl}`, payload).pipe(
      catchError(err => {
        console.error('Create user error', err);
        return of(null);
      })
    );
  }

  deleteUser(id: number): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/${id}`, { observe: 'response' }).pipe(
      map((res: HttpResponse<any>) => res.status === 204 || res.status === 200),
      catchError(err => {
        console.error('Delete user error', err);
        return of(false);
      })
    );
  }

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credentials).pipe(
      catchError(err => {
        console.error('Login error', err);
        return of(null);
      })
    );
  }

  register(payload: { name: string; surname: string; email: string; password: string; contactNumber: string; role: string; }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, payload).pipe(
      catchError(err => {
        console.error('Register error', err);
        return of(null);
      })
    );
  }
}
