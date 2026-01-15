import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, NotificationDto } from '../../core/services/notification.service';
import { RouterLink, Router } from '@angular/router';
import { MissionService } from '../../core/services/mission.service'; // Import MissionService
import { Mission } from '../../core/models/mission.model'; // Import Mission model
import { MissionDetailsModalComponent } from '../missions/components/mission-details-modal/mission-details-modal'; // Import Modal
import { AuthService } from '../../core/services/auth.service'; // Import AuthService

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, RouterLink, MissionDetailsModalComponent], // Add Modal to imports
  templateUrl: './notifications-page.html',
  styles: [`
    .notification-card {
      transition: all 0.2s ease;
      cursor: pointer;
    }
    .notification-card:hover {
      background-color: #f8f9fa;
      transform: translateY(-2px);
    }
    .unread {
      background-color: #e8f0fe !important; /* Light Blue */
      border-right: 4px solid #0d6efd;
    }
    .read {
      background-color: #ffffff;
      opacity: 0.8;
    }
    .icon-box {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
    }
  `]
})
export class NotificationsPage implements OnInit {
  private notify = inject(NotificationService);
  private missionService = inject(MissionService); // Inject MissionService
  private router = inject(Router);
  private authService = inject(AuthService); // Inject AuthService

  notifications = signal<NotificationDto[]>([]);
  loading = signal<boolean>(true);

  // Signal for the modal
  selectedMission = signal<Mission | null>(null);

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.loading.set(true);
    this.notify.getNotifications().subscribe({
      next: (res) => {
        if (res.body) this.notifications.set(res.body);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  markAsRead(n: NotificationDto) {
    if (!n.isRead) {
      this.notify.markAsRead(n.id).subscribe();
      // Optimistic update
      this.notifications.update(list => list.map(item =>
        item.id === n.id ? { ...item, isRead: true } : item
      ));
    }

    if (!n.actionUrl) return;

    // 1. Handle In-Place Modal (New Notifications)
    if (n.actionUrl.includes('openMissionId=')) {
      const urlParams = new URLSearchParams(n.actionUrl.split('?')[1]);
      const mid = urlParams.get('openMissionId');

      if (mid) {
        this.missionService.getMissionById(+mid).subscribe({
          next: (mission) => {
            this.selectedMission.set(mission);
          },
          error: (err) => console.error('Failed to load mission for notification:', err)
        });
        return; // Stay on page
      }
    }

    // 2. Handle Navigation (Legacy & Normal)
    let finalUrl = n.actionUrl;

    // Fix missing prefixes
    if (finalUrl.startsWith('/my-created')) {
      finalUrl = '/missions' + finalUrl;
    } else if (finalUrl.startsWith('/my-missions')) {
      finalUrl = '/missions' + finalUrl;
    }

    // SAFETY CHECK: Prevent Client from going to Courier pages (Legacy Data Fix)
    if (finalUrl.includes('my-missions') && this.authService.hasRole('Client') && !this.authService.hasRole('Courier')) {
      console.warn('Redirecting Client from Courier page to My Created');
      finalUrl = '/missions/my-created';
    }

    console.log('Navigating to:', finalUrl);
    this.router.navigateByUrl(finalUrl);
  }

  markAllRead() {
    this.notify.markAllAsRead().subscribe(() => {
      this.notifications.update(list => list.map(item => ({ ...item, isRead: true })));
    });
  }

  closeMissionDetails() {
    this.selectedMission.set(null);
  }
}
