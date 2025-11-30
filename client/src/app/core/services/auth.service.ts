import { Injectable, signal, computed, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private apiService = inject(ApiService);

  // --- State Management with Signals ---
  // מחזיק את המשתמש הנוכחי (או null אם לא מחובר)
  currentUser = signal<UserProfile | null>(null);

  // נגזרת מחושבת: האם המשתמש מחובר? מתעדכן אוטומטית כשה-currentUser משתנה
  isLoggedIn = computed(() => !!this.currentUser());

  constructor() {
    this.loadUserFromStorage();
  }

  // --- Public Actions ---

  register(data: RegisterRequest): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/register', data).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/login', data).pipe(
      tap(response => this.handleAuthSuccess(response))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUser.set(null); // עדכון ה-Signal
  }

  // קבלת פרטי המשתמש העדכניים מהשרת
  getMe(): Observable<UserProfile> {
    return this.apiService.get<UserProfile>('/users/me');
  }

  // --- Helpers ---

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasRole(role: string): boolean {
    const user = this.currentUser(); // קריאה לערך של ה-Signal
    return user ? user.roles.includes(role) : false;
  }

  // --- Internal Logic ---

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.token);
    this.currentUser.set(response.user); // עדכון ה-Signal
  }

  private loadUserFromStorage(): void {
    const token = this.getToken();
    if (token) {
      // אם יש טוקן, ננסה למשוך את פרטי המשתמש מהשרת
      this.getMe().subscribe({
        next: (user) => this.currentUser.set(user),
        error: () => this.logout()
      });
    }
  }
}