import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environments';
import { AuthResponse } from '../models/auth.model';
import { UserProfile } from '../models/user-profile.model';

const TOKEN_KEY = 'contenthub_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly authState = new BehaviorSubject<boolean>(!!this.getToken());

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, { email, password })
      .pipe(tap(response => {
        this.setToken(response.token);
        this.authState.next(true);
      }));
  }

  register(email: string, displayName: string, password: string) {
    return this.http.post(`${this.baseUrl}/users`, { email, displayName, password });
  }

  me(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/users/me`);
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    this.authState.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return this.authState.value;
  }

  authChanges() {
    return this.authState.asObservable();
  }

  initialize() {
    const token = this.getToken();
    if (!token) {
      this.authState.next(false);
      return;
    }

    this.me().subscribe({
      next: () => this.authState.next(true),
      error: () => this.logout()
    });
  }

  private setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }
}
