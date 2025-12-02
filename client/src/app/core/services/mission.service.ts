import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { Mission, MissionStatus } from '../models/mission.model';

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

  // בעתיד נוסיף כאן:   etc.
    createMission(missionData: Partial<Mission>): Observable<Mission> {
    return this.api.post<Mission>('/missions', missionData);
    }

    acceptMission(missionId: number): Observable<void> {
    // ה-API הוא PUT /api/missions/{id}/accept
    // אנחנו לא שולחים body, לכן {} ריק
    return this.api.put<void>(`/missions/${missionId}/accept`, {});
  }

  getMyAssignedMissions(): Observable<Mission[]> {
    return this.api.get<Mission[]>('/missions/my-assigned');
  }
  
  updateMissionStatus(id: number, status: MissionStatus): Observable<Mission> {
    return this.api.put<Mission>(`/missions/${id}/status`, { status });
  }
}