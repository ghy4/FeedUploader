import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface User {
  id: number;
  name: string;
  surname: string;
  email: string;
  contactNumber: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  surname: string;
  email: string;
  password: string;
  contactNumber: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7281/api/User'; // Match your backend UserController route
  private currentUserKey = 'currentUser';
  private tokenKey = 'authToken';

  constructor(private http: HttpClient) { }

  // Login method
  login(credentials: LoginRequest): Observable<{ token: string; user: User }> {
    return this.http.post<{ token: string; user: User }>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        // Store token and user
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.currentUserKey, JSON.stringify(response.user));
      }),
      catchError(error => {
        console.error('Login error:', error);
        throw new Error(error.error?.message || 'Invalid credentials');
      })
    );
  }

  // Register method
  register(userData: RegisterRequest): Observable<{ message: string; user: User; token?: string }> {
    return this.http.post<{ message: string; user: User; token?: string }>(`${this.apiUrl}/register`, userData).pipe(
      tap(response => {
        // Store user and token if provided
        localStorage.setItem(this.currentUserKey, JSON.stringify(response.user));
        if (response.token) {
          localStorage.setItem(this.tokenKey, response.token);
        }
      }),
      catchError(error => {
        const msg = error.error?.message || error.message || 'Registration failed';
        console.error('Registration error:', msg);
        throw new Error(msg);
      })
    );
  }

  // Logout method
  logout(): void {
    localStorage.removeItem(this.currentUserKey);
    localStorage.removeItem(this.tokenKey);
  }

  // Get current user
  getCurrentUser(): User | null {
    const userString = localStorage.getItem(this.currentUserKey);
    return userString ? JSON.parse(userString) : null;
  }

  // Check if user is logged in
  isLoggedIn(): boolean {
    return !!this.getCurrentUser() && !!localStorage.getItem(this.tokenKey);
  }

  // Get authentication token (for HTTP headers if needed)
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUserRole(): string | null {
    return this.getCurrentUser()?.role || null;
  }

  isAdmin(): boolean {
    return this.getUserRole() === 'Admin' || this.getUserRole() === 'admin';
  }

  hasRole(required: string | string[]): boolean {
    const role = this.getUserRole();
    if (!role) return false;
    if (Array.isArray(required)) return required.some(r => r.toLowerCase() === role.toLowerCase());
    return role.toLowerCase() === required.toLowerCase();
  }
}
