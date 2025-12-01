import { Routes } from '@angular/router';
import { MissionDashboard } from './mission-dashboard/mission-dashboard';
import { MissionForm } from './mission-form/mission-form';
import { roleGuard } from '../../core/guards/role.guard';

export const MISSIONS_ROUTES: Routes = [
  { 
    path: 'dashboard', 
    component: MissionDashboard 
  },
  { 
    path: 'create', 
    component: MissionForm,
    canActivate: [roleGuard], // מפעיל את השומר לפני הכניסה לנתיב
    data: { role: 'Client' }  
  },
  { 
    path: '', 
    redirectTo: 'dashboard', 
    pathMatch: 'full' 
  }
];