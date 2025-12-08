import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MissionService } from '../../../core/services/mission.service';
import { Mission, MissionStatus } from '../../../core/models/mission.model';
import { MissionStatusPipe } from '../../../shared/pipes/mission-status.pipe';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';
import { RouterLink } from '@angular/router';
import { MissionRequestsModalComponent } from '../components/mission-requests-modal/mission-requests-modal';

@Component({
  selector: 'app-my-created-missions',
  templateUrl: './my-created-missions.html',
  styleUrl: './my-created-missions.scss',
  standalone: true,
  imports: [CommonModule, MissionStatusPipe, PackageSizePipe, RouterLink, MissionRequestsModalComponent]
})
export class MyCreatedMissions implements OnInit {
  private missionService = inject(MissionService);

  missions = signal<Mission[]>([]);
  isLoading = signal<boolean>(true);
  selectedMissionId = signal<number | null>(null);

  MissionStatus = MissionStatus;

  ngOnInit(): void {
    this.loadMissions();
  }

  loadMissions(): void {
    this.isLoading.set(true);
    this.missionService.getMyCreatedMissions().subscribe({
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

  openRequests(id: number): void {
    this.selectedMissionId.set(id);
  }

  closeRequests(): void {
    this.selectedMissionId.set(null);
    this.loadMissions(); // Reload to see status changes if approved
  }
}
