import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER, inject } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthService } from './core/services/auth.service';
import { Observable } from 'rxjs';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

function initializeAuth(): () => Observable<void> {
  return () => {
    const authService = inject(AuthService);
    return authService.attemptAutoLogin();
  };
}

import { provideServiceWorker } from '@angular/service-worker';
import { initializeApp, provideFirebaseApp } from '@angular/fire/app';
import { provideMessaging, getMessaging } from '@angular/fire/messaging';
import { isDevMode } from '@angular/core';

const firebaseConfig = {
  apiKey: "AIzaSyAMQ4rNIJTEoHhlmU0mJSTq4eWpHReHz2k",
  authDomain: "tavili-93a96.firebaseapp.com",
  projectId: "tavili-93a96",
  storageBucket: "tavili-93a96.firebasestorage.app",
  messagingSenderId: "816270053530",
  appId: "1:816270053530:web:ecafda7212c699b7730f23"
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    provideFirebaseApp(() => initializeApp(firebaseConfig)),
    provideMessaging(() => getMessaging()),
    // provideServiceWorker removed to avoid conflict with AngularFire Messaging which handles its own SW registration.
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAuth,
      multi: true
    }
  ]
};
