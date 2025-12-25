import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription, startWith, switchMap } from 'rxjs';
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
export class MyCreatedMissions implements OnInit, OnDestroy {
  private missionService = inject(MissionService);

  missions = signal<Mission[]>([]);
  isLoading = signal<boolean>(true);
  selectedMissionId = signal<number | null>(null);

  private pollSub: Subscription | null = null;

  MissionStatus = MissionStatus;

  ngOnInit(): void {
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  startPolling(): void {
    this.isLoading.set(true);
    // Poll every 5 seconds (more frequent for user's own items)
    this.pollSub = interval(5000)
      .pipe(
        startWith(0),
        switchMap(() => this.missionService.getMyCreatedMissions())
      )
      .subscribe({
        next: (data) => {
          this.missions.set(data);
          // Only stop loading spinner on first load or if it's still running
          if (this.isLoading()) {
            this.isLoading.set(false);
          }
        },
        error: (err) => {
          console.error('Polling error:', err);
          this.isLoading.set(false);
        }
      });
  }

  stopPolling(): void {
    if (this.pollSub) {
      this.pollSub.unsubscribe();
      this.pollSub = null;
    }
  }

  // Helper for manual refresh if needed (e.g. after closing modal)
  manualRefresh(): void {
    this.missionService.getMyCreatedMissions().subscribe(data => this.missions.set(data));
  }

  openRequests(id: number): void {
    this.selectedMissionId.set(id);
  }

  closeRequests(): void {
    this.selectedMissionId.set(null);
    this.manualRefresh(); // Reload immediate state while polling continues
  }
}
