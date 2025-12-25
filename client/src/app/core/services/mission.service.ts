import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { Mission, MissionStatus } from '../models/mission.model';

@Injectable({
  providedIn: 'root'
})
export class MissionService {
  private api = inject(ApiService);

  // שליפת כל המשימות הפתוחות - תומך בסינון אופציונלי
  getOpenMissions(filters?: {
    pickupCity?: string,
    dropoffCity?: string,
    relatedCity?: string
  }): Observable<Mission[]> {

    // בניית פרמטרים ל-URL
    let params = new HttpParams();
    if (filters) {
      if (filters.pickupCity) params = params.set('pickupCity', filters.pickupCity);
      if (filters.dropoffCity) params = params.set('dropoffCity', filters.dropoffCity);
      if (filters.relatedCity) params = params.set('relatedCity', filters.relatedCity);
    }

    return this.api.get<Mission[]>('/missions/open', params);
  }

  createMission(missionData: Partial<Mission>): Observable<Mission> {
    return this.api.post<Mission>('/missions', missionData);
  }

  acceptMission(missionId: number): Observable<void> {
    // ה-API הוא PUT /api/missions/{id}/accept
    // אנחנו לא שולחים body, לכן {} ריק
    return this.api.put<void>(`/missions/${missionId}/accept`, {});
  }

  getMyAssignedMissions(): Observable<Mission[]> {
    const params = new HttpParams().set('t', new Date().getTime().toString());
    return this.api.get<Mission[]>('/missions/my-assigned', params);
  }

  updateMissionStatus(id: number, status: MissionStatus): Observable<Mission> {
    return this.api.put<Mission>(`/missions/${id}/status`, { status });
  }

  getMyCreatedMissions(): Observable<Mission[]> {
    const params = new HttpParams().set('t', new Date().getTime().toString());
    return this.api.get<Mission[]>('/missions/my-created', params);
  }

  requestMission(missionId: number): Observable<number> {
    return this.api.post<number>(`/missions/${missionId}/request`, {});
  }

  getMissionRequests(missionId: number): Observable<any[]> {
    return this.api.get<any[]>(`/missions/${missionId}/requests`);
  }

  approveRequest(requestId: number): Observable<void> {
    return this.api.post<void>(`/missions/requests/${requestId}/approve`, {});
  }
}