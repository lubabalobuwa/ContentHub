import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, BehaviorSubject, shareReplay, throwError } from 'rxjs';
import { environment } from '../../../environments/environments';
import { AuthResponse } from '../models/auth.model';
import { UserProfile } from '../models/user-profile.model';

const TOKEN_KEY = 'contenthub_token';
const REFRESH_TOKEN_KEY = 'contenthub_refresh_token';
const ROLE_KEY = 'contenthub_role';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly authState = new BehaviorSubject<boolean>(!!this.getToken());
  private refreshInFlight$: Observable<AuthResponse> | null = null;

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, { email, password })
      .pipe(tap(response => {
        this.setToken(response.token);
        this.setRefreshToken(response.refreshToken);
        this.setRole(response.role);
        this.authState.next(true);
      }));
  }

  externalLogin(provider: 'google') {
    window.location.href = `${this.baseUrl}/auth/external/${provider}`;
  }

  exchangeExternalLogin(code: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/external/exchange`, { code })
      .pipe(tap(response => {
        this.setToken(response.token);
        this.setRefreshToken(response.refreshToken);
        this.setRole(response.role);
        this.authState.next(true);
      }));
  }

  refresh(): Observable<AuthResponse> {
    if (this.refreshInFlight$) return this.refreshInFlight$;
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available.'));
    }

    this.refreshInFlight$ = this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.setToken(response.token);
          this.setRefreshToken(response.refreshToken);
          this.setRole(response.role);
          this.authState.next(true);
        }),
        shareReplay(1)
      );

    this.refreshInFlight$.subscribe({
      complete: () => {
        this.refreshInFlight$ = null;
      },
      error: () => {
        this.refreshInFlight$ = null;
      }
    });

    return this.refreshInFlight$;
  }

  register(email: string, displayName: string, password: string, turnstileToken: string) {
    return this.http.post(`${this.baseUrl}/users`, { email, displayName, password, turnstileToken });
  }

  resendVerification(email: string) {
    return this.http.post(`${this.baseUrl}/auth/resend-verification`, { email });
  }

  verifyEmail(token: string) {
    return this.http.get(`${this.baseUrl}/auth/verify-email`, { params: { token } });
  }

  forgotPassword(email: string) {
    return this.http.post(`${this.baseUrl}/auth/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string) {
    return this.http.post(`${this.baseUrl}/auth/reset-password`, { token, newPassword });
  }

  me(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/users/me`);
  }

  logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    this.authState.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return this.authState.value;
  }

  getRole(): string | null {
    return localStorage.getItem(ROLE_KEY);
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
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
      next: profile => {
        this.setRole(profile.role);
        this.authState.next(true);
      },
      error: () => this.logout()
    });
  }

  private setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  private setRefreshToken(token: string) {
    localStorage.setItem(REFRESH_TOKEN_KEY, token);
  }

  private setRole(role: string) {
    localStorage.setItem(ROLE_KEY, role);
  }
}
