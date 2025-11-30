import { Routes } from '@angular/router';
import { MissionDashboard } from './mission-dashboard/mission-dashboard';

export const MISSIONS_ROUTES: Routes = [
  { path: 'dashboard', component: MissionDashboard },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];