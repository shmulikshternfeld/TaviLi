import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MissionService } from '../../../core/services/mission.service';
import { Mission } from '../../../core/models/mission.model';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-mission-dashboard',
  templateUrl: './mission-dashboard.html',
  styleUrl: './mission-dashboard.scss',
  standalone: true,
  imports: [CommonModule, PackageSizePipe] // ייבוא ה-Pipe וה-CommonModule
})
export class MissionDashboard implements OnInit {
  private missionService = inject(MissionService);
  private authService = inject(AuthService);
  private router = inject(Router);

  // סיגנל שמחזיק את רשימת המשימות
  missions = signal<Mission[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.loadMissions();
  }

  loadMissions(): void {
    this.isLoading.set(true);
    
    this.missionService.getOpenMissions().subscribe({
      next: (data) => {
        this.missions.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading missions:', err);
        this.isLoading.set(false);
      }
    });
  }
  // --- פעולות הדורשות אימות ---
  onCreateMissionClick(): void {
    if (!this.authService.isLoggedIn()) {
      this.redirectToLogin();
      return;
    }
    // TODO: נווט לטופס יצירה (כשיהיה לנו כזה)
    console.log('Navigate to create mission...');
  }

  onAcceptMissionClick(mission: Mission): void {
    if (!this.authService.isLoggedIn()) {
      this.redirectToLogin();
      return;
    }
    // TODO: לוגיקת קבלת משלוח (קריאה ל-API)
    console.log('Accept mission:', mission.id);
  }

  private redirectToLogin(): void {
    // אפשר להוסיף כאן גם הודעת SweetAlert יפה: "עליך להתחבר כדי לבצע פעולה זו"
    this.router.navigate(['/auth/login']);
  }
}
