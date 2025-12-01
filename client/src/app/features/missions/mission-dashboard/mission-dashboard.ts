import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router} from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { AuthService } from '../../../core/services/auth.service';
import { Mission } from '../../../core/models/mission.model';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';

@Component({
  selector: 'app-mission-dashboard',
  templateUrl: './mission-dashboard.html',
  styleUrl: './mission-dashboard.scss',
  standalone: true,
  imports: [CommonModule, PackageSizePipe] 
})
export class MissionDashboard implements OnInit {
    private missionService = inject(MissionService);
    private authService = inject(AuthService);
    private router = inject(Router);
  
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
          console.error(err);
          this.isLoading.set(false);
        }
      });
    }
  
    onCreateMissionClick(): void {
      if (!this.authService.isLoggedIn()) {
        this.router.navigate(['/auth/login']);
        return;
      }
  
      if (this.authService.hasRole('Client')) {
        this.router.navigate(['/missions/create']);
      } else {
        alert('פעולה זו זמינה ללקוחות רשומים בלבד. אנא הירשם כלקוח.'); 
      }
    }
  
    onAcceptMissionClick(mission: Mission): void {
      if (!this.authService.isLoggedIn()) {
        this.router.navigate(['/auth/login']);
        return;
      }
      console.log('Accept mission:', mission.id);
    }
}
