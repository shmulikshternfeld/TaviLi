import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subscription, startWith, switchMap } from 'rxjs';
import { MissionService } from '../../../core/services/mission.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Mission, MissionStatus, MissionRequestStatus } from '../../../core/models/mission.model';
import { MissionStatusPipe } from '../../../shared/pipes/mission-status.pipe';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';

@Component({
  selector: 'app-my-missions',
  templateUrl: './my-missions.html',
  styleUrl: './my-missions.scss',
  standalone: true,
  imports: [CommonModule, MissionStatusPipe, PackageSizePipe]
})
export class MyMissions implements OnInit, OnDestroy {
  private missionService = inject(MissionService);
  private notify = inject(NotificationService);

  missions = signal<Mission[]>([]);
  isLoading = signal<boolean>(true);

  private pollSub: Subscription | null = null;

  // כדי להשתמש ב-Enum ב-HTML
  MissionStatus = MissionStatus;
  MissionRequestStatus = MissionRequestStatus;

  ngOnInit(): void {
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  startPolling(): void {
    this.isLoading.set(true);
    // Poll every 5 seconds to catch new assignments quickly
    this.pollSub = interval(5000)
      .pipe(
        startWith(0),
        switchMap(() => this.missionService.getMyAssignedMissions())
      )
      .subscribe({
        next: (data) => {
          this.missions.set(data);
          // Stop loading spinner if it's running
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

  manualRefresh(): void {
    this.missionService.getMyAssignedMissions().subscribe(data => this.missions.set(data));
  }

  // קידום הסטטוס לשלב הבא
  async advanceStatus(mission: Mission): Promise<void> {
    let nextStatus: MissionStatus;
    let confirmText = '';

    // לוגיקה פשוטה לקידום הסטטוס (State Machine)
    switch (mission.status) {
      case MissionStatus.Accepted:
        nextStatus = MissionStatus.InProgress_Pickup;
        confirmText = 'האם אתה יוצא לאיסוף כעת?';
        break;
      case MissionStatus.InProgress_Pickup:
        nextStatus = MissionStatus.Collected;
        confirmText = 'האם החבילה נאספה ונמצאת אצלך?';
        break;
      case MissionStatus.Collected:
        nextStatus = MissionStatus.InProgress_Delivery;
        confirmText = 'האם אתה יוצא למסירה כעת?';
        break;
      case MissionStatus.InProgress_Delivery:
        nextStatus = MissionStatus.Completed;
        confirmText = 'האם החבילה נמסרה ליעד בהצלחה?';
        break;
      default:
        return; // סטטוס סופי או לא ידוע
    }

    const confirmed = await this.notify.confirm('עדכון סטטוס', confirmText, 'כן, עדכן');
    if (!confirmed) return;

    this.missionService.updateMissionStatus(mission.id, nextStatus).subscribe({
      next: () => {
        this.notify.success('הסטטוס עודכן בהצלחה');
        this.manualRefresh(); // רענון הנתונים
      },
      error: (err) => {
        console.error(err);
        this.notify.error('שגיאה', 'לא ניתן היה לעדכן את הסטטוס');
      }
    });
  }
}
