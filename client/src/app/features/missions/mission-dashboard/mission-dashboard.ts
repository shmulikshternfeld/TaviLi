import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { AuthService } from '../../../core/services/auth.service';
import { Mission, PackageSize } from '../../../core/models/mission.model';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';
import { NotificationService } from '../../../core/services/notification.service';
import { MissionFilterComponent, FilterState } from '../components/mission-filter/mission-filter';
import { MissionDetailsModalComponent } from '../components/mission-details-modal/mission-details-modal';
import { interval, Subscription, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-mission-dashboard',
  templateUrl: './mission-dashboard.html',
  styleUrl: './mission-dashboard.scss',
  standalone: true,
  imports: [CommonModule, RouterModule, PackageSizePipe, MissionFilterComponent, MissionDetailsModalComponent]
})
export class MissionDashboard implements OnInit, OnDestroy {
  private missionService = inject(MissionService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  // Server Data
  private allFetchedMissions = signal<Mission[]>([]);

  // State
  private pollSub: Subscription | null = null;
  private currentServerFilters: { relatedCity?: string, pickupCity?: string, dropoffCity?: string } = {};

  // Filters
  private currentPackageSizeFilter = signal<string | PackageSize>('All');

  // Display Data
  missions = computed(() => {
    const sizeFilter = this.currentPackageSizeFilter();
    const all = this.allFetchedMissions();

    if (sizeFilter === 'All') return all;
    return all.filter(m => m.packageSize === sizeFilter);
  });

  isLoading = signal<boolean>(true);
  busyMissionId = signal<number | null>(null);

  // Modal State
  selectedMission = signal<Mission | null>(null);
  isRequesting = signal<boolean>(false);
  hasAlreadyRequested = signal<boolean>(false);

  ngOnInit(): void {
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  startPolling(): void {
    // Poll every 10 seconds
    this.pollSub = interval(10000)
      .pipe(
        startWith(0),
        switchMap(() => {
          // We pass current filters to the service
          return this.missionService.getOpenMissions(this.currentServerFilters);
        })
      )
      .subscribe({
        next: (data) => {
          this.allFetchedMissions.set(data);
          this.isLoading.set(false); // Only relevant for first load
        },
        error: (err) => {
          console.error(err);
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

  // Called manually by filter component
  onFilterChange(event: FilterState) {
    this.currentPackageSizeFilter.set(event.packageSize);

    // Update filters and restart polling to trigger immediate fetch with new filters
    this.currentServerFilters = {
      relatedCity: event.relatedCity,
      pickupCity: event.pickupCity,
      dropoffCity: event.dropoffCity
    };

    // Restart polling to apply new filers immediately and reset interval
    this.stopPolling();
    this.isLoading.set(true); // Show spinner for manual filter change
    this.startPolling();
  }

  onCreateMissionClick(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    if (this.authService.hasRole('Client')) {
      this.router.navigate(['/missions/create']);
    } else {
      this.notify.error('חסרה הרשאה', 'פעולה זו זמינה ללקוחות רשומים בלבד.');
    }
  }

  onDetailsClick(mission: Mission): void {
    this.selectedMission.set(mission);
    this.hasAlreadyRequested.set(false);
    // Ideally we verify if user requested it, but we'll handle existing request error on click
  }

  closeModal(): void {
    this.selectedMission.set(null);
  }

  onRequestMission(): void {
    const mission = this.selectedMission();
    if (!mission) return;

    // 1. Check Login
    if (!this.authService.isLoggedIn()) {
      this.notify.error('התחברות נדרשת', 'עליך להתחבר למערכת כדי לבקש משלוחים');
      this.router.navigate(['/auth/login']);
      this.closeModal();
      return;
    }

    // 2. Check Role
    if (!this.authService.hasRole('Courier')) {
      this.notify.error('חסרה הרשאה', 'רק משתמשים הרשומים כשליחים יכולים לקבל משימות.');
      return;
    }

    // 3. Check Self-Request
    const currentUserId = this.authService.currentUser()?.id;
    if (mission.creatorUserId === currentUserId) {
      this.notify.error('פעולה לא חוקית', 'לא ניתן לקבל משלוח שאתה יצרת בעצמך 😅');
      return;
    }

    // 4. Execute
    this.isRequesting.set(true);

    this.missionService.requestMission(mission.id).subscribe({
      next: (requestId) => {
        this.notify.success('הבקשה נשלחה בהצלחה! 🚀');
        this.hasAlreadyRequested.set(true);
        this.isRequesting.set(false);
        setTimeout(() => this.closeModal(), 2000);
      },
      error: (err) => {
        console.error(err);
        this.isRequesting.set(false);
        if (err.error?.message?.includes("Already requested") || err.status === 400) {
          this.hasAlreadyRequested.set(true);
          this.notify.error('כבר ביקשת', 'כבר שלחת בקשה למשלוח זה.');
        } else {
          this.notify.error('אופס...', 'אירעה שגיאה בשליחת הבקשה.');
        }
      }
    });
  }
}
