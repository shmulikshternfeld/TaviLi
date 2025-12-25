import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthService } from './core/services/auth.service';
import { Observable } from 'rxjs';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

function initializeApp(): () => Observable<void> {
  return () => {
    const authService = inject(AuthService);
    return authService.attemptAutoLogin();
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // הגדרת ה-HTTP Client עם האינטרספטור של האותנטיקציה
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      multi: true
    }
  ]
};
