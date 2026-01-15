import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  // 1. בדיקה בסיסית: האם מחובר?
  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/auth/login']);
  }

  // 2. שליפת התפקיד הנדרש מתוך הגדרת ה-Route
  const requiredRole = route.data['role'] as string;

  // 3. בדיקה האם למשתמש יש את התפקיד
  if (authService.hasRole(requiredRole)) {
    return true; // יש אישור
  }

  // 4. אם המשתמש מחובר אך אין לו הרשאה
  const roleNames: Record<string, string> = {
    'Client': 'לקוחות',
    'Courier': 'שליחים',
    'Admin': 'מנהלים'
  };
  const hebrewRole = roleNames[requiredRole] || requiredRole;

  notificationService.error('אין לך הרשאה', `אזור זה מיועד ל${hebrewRole} בלבד`);
  return router.createUrlTree(['/missions/dashboard']);
};