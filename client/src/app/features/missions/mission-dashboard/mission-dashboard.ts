import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { AuthService } from '../../../core/services/auth.service';
import { Mission, PackageSize } from '../../../core/models/mission.model';
import { PackageSizePipe } from '../../../shared/pipes/package-size.pipe';
import { NotificationService } from '../../../core/services/notification.service';
import { MissionFilterComponent, FilterState } from '../components/mission-filter/mission-filter';

@Component({
  selector: 'app-mission-dashboard',
  templateUrl: './mission-dashboard.html',
  styleUrl: './mission-dashboard.scss',
  standalone: true,
  imports: [CommonModule, PackageSizePipe, MissionFilterComponent]
})
export class MissionDashboard implements OnInit {
  private missionService = inject(MissionService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notify = inject(NotificationService);

  // נתונים מהשרת
  private allFetchedMissions = signal<Mission[]>([]);

  // פילטרים נוכחיים
  private currentPackageSizeFilter = signal<string | PackageSize>('All');

  // נתונים לתצוגה (אחרי סינון קליינט)
  missions = computed(() => {
    const sizeFilter = this.currentPackageSizeFilter();
    const all = this.allFetchedMissions();

    if (sizeFilter === 'All') return all;
    return all.filter(m => m.packageSize === sizeFilter);
  });

  isLoading = signal<boolean>(true);
  busyMissionId = signal<number | null>(null);

  ngOnInit(): void {
    this.loadMissions();
  }

  // שליפה מהשרת עם סינון ערים
  loadMissions(filters?: { relatedCity?: string, pickupCity?: string, dropoffCity?: string }): void {
    this.isLoading.set(true);

    // המרת הפילטר שלנו לפורמט של הסרוויס
    const serviceFilters: any = {};
    if (filters?.relatedCity) serviceFilters.relatedCity = filters.relatedCity;
    if (filters?.pickupCity) serviceFilters.pickupCity = filters.pickupCity;
    if (filters?.dropoffCity) serviceFilters.dropoffCity = filters.dropoffCity;

    this.missionService.getOpenMissions(serviceFilters).subscribe({
      next: (data) => {
        this.allFetchedMissions.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.isLoading.set(false);
      }
    });
  }

  onFilterChange(event: FilterState) {
    // 1. עדכון פילטר גודל חבילה (לקוח)
    this.currentPackageSizeFilter.set(event.packageSize);

    // 2. שליפה מחדש עם הפילטרים המעודכנים (שרת)
    this.loadMissions({
      relatedCity: event.relatedCity,
      pickupCity: event.pickupCity,
      dropoffCity: event.dropoffCity
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
      this.notify.error('חסרה הרשאה', 'פעולה זו זמינה ללקוחות רשומים בלבד.');
    }
  }

  async onAcceptMissionClick(mission: Mission): Promise<void> {
    // 1. בדיקת התחברות
    if (!this.authService.isLoggedIn()) {
      this.notify.error('התחברות נדרשת', 'עליך להתחבר למערכת כדי לקבל משלוחים');
      this.router.navigate(['/auth/login']);
      return;
    }

    // 2. בדיקת תפקיד
    if (!this.authService.hasRole('Courier')) {
      this.notify.error('חסרה הרשאה', 'רק משתמשים הרשומים כשליחים יכולים לקבל משימות.');
      return;
    }

    // 3. בדיקת "משלוח עצמי"
    const currentUserId = this.authService.currentUser()?.id;
    if (mission.creatorUserId === currentUserId) {
      this.notify.error('פעולה לא חוקית', 'לא ניתן לקבל משלוח שאתה יצרת בעצמך 😅');
      return;
    }

    // 4. אישור המשתמש
    const confirmed = await this.notify.confirm(
      'קבלת משלוח',
      `האם אתה בטוח שברצונך לקחת את המשלוח מ-${mission.pickupAddress}?`
    );

    if (!confirmed) return;

    // 5. ביצוע הפעולה
    this.busyMissionId.set(mission.id);

    this.missionService.acceptMission(mission.id).subscribe({
      next: () => {
        this.notify.success('המשלוח שויך אליך בהצלחה! 🚀');
        this.loadMissions();
        this.busyMissionId.set(null);
      },
      error: (err) => {
        console.error(err);
        this.notify.error('אופס...', 'אירעה שגיאה בקבלת המשלוח. ייתכן שהוא כבר נתפס.');
        this.busyMissionId.set(null);
        this.loadMissions();
      }
    });
  }
}
