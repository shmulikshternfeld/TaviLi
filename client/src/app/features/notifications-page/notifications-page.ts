import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, NotificationDto } from '../../core/services/notification.service';
import { RouterLink, Router } from '@angular/router';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
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

  notifications = signal<NotificationDto[]>([]);
  loading = signal<boolean>(true);

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

  private router = inject(Router);

  markAsRead(n: NotificationDto) {
    if (!n.isRead) {
      this.notify.markAsRead(n.id).subscribe();
      // Optimistic update
      this.notifications.update(list => list.map(item =>
        item.id === n.id ? { ...item, isRead: true } : item
      ));
    }

    if (n.actionUrl) {
      // Patch legacy URLs (from before the fix)
      let finalUrl = n.actionUrl;
      if (finalUrl.startsWith('/my-created')) {
        finalUrl = '/missions' + finalUrl;
      } else if (finalUrl.startsWith('/my-missions')) {
        finalUrl = '/missions' + finalUrl;
      }

      console.log('Navigating to:', finalUrl);
      this.router.navigateByUrl(finalUrl);
    }
  }

  markAllRead() {
    this.notify.markAllAsRead().subscribe(() => {
      this.notifications.update(list => list.map(item => ({ ...item, isRead: true })));
    });
  }
}
