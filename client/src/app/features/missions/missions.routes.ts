import { Routes } from '@angular/router';
import { MissionDashboard } from './mission-dashboard/mission-dashboard';
import { MissionForm } from './mission-form/mission-form';
import { roleGuard } from '../../core/guards/role.guard';
import { MyMissions } from './my-missions/my-missions';
import { MyCreatedMissions } from './my-created-missions/my-created-missions';

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
    path: 'my-missions',
    component: MyMissions,
    canActivate: [roleGuard],
    data: { role: 'Courier' } // חובה להיות שליח
  },
  {
    path: 'my-created', 
    component: MyCreatedMissions,
    canActivate: [roleGuard],
    data: { role: 'Client' } // רק לקוחות
  },
  { 
    path: '', 
    redirectTo: 'dashboard', 
    pathMatch: 'full' 
  }
];