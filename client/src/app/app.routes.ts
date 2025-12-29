import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'missions',
    loadChildren: () => import('./features/missions/missions.routes').then(m => m.MISSIONS_ROUTES)
  },
  {
    path: '',
    redirectTo: 'missions',
    pathMatch: 'full'
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/users/profile/profile').then(m => m.ProfileComponent)
  },
  {
    path: 'profile/:id',
    loadComponent: () => import('./features/users/profile/profile').then(m => m.ProfileComponent)
  },
  {
    path: 'notifications',
    loadComponent: () => import('./features/notifications-page/notifications-page').then(m => m.NotificationsPage)
  }
];
