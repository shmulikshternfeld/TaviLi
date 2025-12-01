import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { Mission } from '../models/mission.model';

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private api = inject(ApiService);

  // שליפת כל המשימות הפתוחות
  getOpenMissions(): Observable<Mission[]> {
    // נתיב ה-API שבנינו ב-Backend: GET /api/missions/open
    return this.api.get<Mission[]>('/missions/open');
  }

  // בעתיד נוסיף כאן: createMission, acceptMission, etc.
}