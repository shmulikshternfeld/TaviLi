import { Component, signal, inject, OnInit, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './core/layout/navbar/navbar';
import { NotificationService } from './core/services/notification.service';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('client');
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  constructor() {
    // Automatically initialize notifications when user logs in (or session restores)
    effect(() => {
      const user = this.authService.currentUser();
      if (user) {
        // User just logged in or session restored
        this.notificationService.initialize();
        this.notificationService.getNotifications().subscribe();
      } else {
        // User logged out
        // Optional: clear notifications or unsubscribe
      }
    });
  }

  ngOnInit() {
    // Initial check (optional/redundant due to effect, but keeps logic clean)
  }
}
