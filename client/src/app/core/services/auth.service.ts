import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { LoginRequest, RegisterRequest, AuthResponse, UserProfile } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  
  // מחזיק את המצב הנוכחי של המשתמש
  private currentUserSubject = new BehaviorSubject<UserProfile | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private apiService: ApiService) {
    this.loadUserFromStorage();
  }

  // בדיקה ראשונית בעליית האפליקציה אם יש טוקן שמור
  private loadUserFromStorage(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    if (token) {
      // אופציונלי: כאן ניתן להוסיף קריאה ל-GET /users/me כדי לוודא שהטוקן עדיין בתוקף
      // ל-MVP, נניח שאם יש טוקן אנחנו מחוברים (או נפענח אותו אם צריך פרטים מיידית)
      // לצורך הדוגמה נבצע קריאה לשרת לרענון פרטים:
      this.getMe().subscribe({
         next: (user) => this.currentUserSubject.next(user),
         error: () => this.logout() // אם הטוקן לא תקין, נתנתק
      });
    }
  }

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
    this.currentUserSubject.next(null);
    // כאן אפשר להוסיף ניווט לדף הבית
  }

  private handleAuthSuccess(response: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.token);
    this.currentUserSubject.next(response.user);
  }

  // קבלת פרטי המשתמש הנוכחי מהשרת
  getMe(): Observable<UserProfile> {
    return this.apiService.get<UserProfile>('/users/me');
  }

  // עזרים (Helpers)
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken(); // מחזיר true אם יש טוקן
  }
  
  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.roles.includes(role) : false;
  }
}