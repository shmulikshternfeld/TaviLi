import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription, startWith, switchMap } from 'rxjs';
import { MissionService } from '../../../core/services/mission.service';
import { Mission, MissionStatus } from '../../../core/models/mission.model';
import { MissionStatusPipe } from '../../../shared/pipes/mission-status.pipe';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { MissionRequestsModalComponent } from '../components/mission-requests-modal/mission-requests-modal';
import { MissionDetailsModalComponent } from '../components/mission-details-modal/mission-details-modal';

@Component({
  selector: 'app-my-created-missions',
  templateUrl: './my-created-missions.html',
  styleUrl: './my-created-missions.scss',
  standalone: true,
  imports: [CommonModule, MissionStatusPipe, PackageSizePipe, RouterLink, MissionRequestsModalComponent, MissionDetailsModalComponent]
})
export class MyCreatedMissions implements OnInit, OnDestroy {
  private missionService = inject(MissionService);
  private router = inject(Router);

  missions = signal<Mission[]>([]);
  isLoading = signal<boolean>(true);

  // For Courier Requests Modal
  selectedMissionId = signal<number | null>(null);

  // For Details Modal (Client Updates)
  selectedDetailsMission = signal<Mission | null>(null);

  private pollSub: Subscription | null = null;

  MissionStatus = MissionStatus;

  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    this.startPolling();

    // Check for deep links (e.g. from notifications)
    this.route.queryParams.subscribe(params => {
      console.log('MyCreatedMissions: QueryParams changed', params);

      const openReq = params['openRequests'];
      const openMissionId = params['openMissionId'];
      const mid = params['missionId'];

      if (mid && openReq === 'true') {
        this.selectedMissionId.set(+mid);
      } else if (openMissionId) {
        // User clicked typical "Update" notification
        // We need to fetch this mission to show details
        this.openMissionDetails(+openMissionId);
      }
    });
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
          if (this.isLoading()) {
            this.isLoading.set(false);
          }

          // If we have a pending details request (from URL) and data just arrived, maybe we can find it now?
          // Actually openMissionDetails fetches individually if needed.
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

  manualRefresh(): void {
    this.missionService.getMyCreatedMissions().subscribe(data => this.missions.set(data));
  }

  openRequests(id: number): void {
    this.selectedMissionId.set(id);
  }

  closeRequests(): void {
    this.selectedMissionId.set(null);
    this.manualRefresh();
  }

  openMissionDetails(id: number): void {
    // 1. Try to find in current list
    const found = this.missions().find(m => m.id === id);
    if (found) {
      this.selectedDetailsMission.set(found);
    } else {
      console.warn('Mission not found in local list. ID:', id);
      // Optional: Force a refresh and try again? 
      // For now, simpler is safer.
    }
  }

  closeMissionDetails(): void {
    this.selectedDetailsMission.set(null);
    // Remove query params to prevent reopening on refresh
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { openMissionId: null },
      queryParamsHandling: 'merge'
    });
  }
}
