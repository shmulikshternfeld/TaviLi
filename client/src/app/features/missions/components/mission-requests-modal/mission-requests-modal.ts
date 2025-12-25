import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, inject, signal, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { interval, Subscription, switchMap, startWith } from 'rxjs';
import { MissionService } from '../../../../core/services/mission.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MissionRequest, MissionRequestStatus } from '../../../../core/models/mission.model';

@Component({
    selector: 'app-mission-requests-modal',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './mission-requests-modal.html',
    styleUrls: ['./mission-requests-modal.scss']
})
export class MissionRequestsModalComponent implements OnInit, OnDestroy {
    @Input() missionId!: number;
    @Output() close = new EventEmitter<void>();
    @Output() approved = new EventEmitter<void>(); // Emit when a request is approved

    private missionService = inject(MissionService);
    private notify = inject(NotificationService);
    private cd = inject(ChangeDetectorRef);
    private pollSub: Subscription | null = null;

    requests = signal<MissionRequest[]>([]);
    isLoading = signal<boolean>(true);
    isProcessing = signal<number | null>(null);

    ngOnInit(): void {
        this.startPolling();
    }

    ngOnDestroy(): void {
        this.stopPolling();
    }

    startPolling(): void {
        this.isLoading.set(true);
        // Poll every 5 seconds
        this.pollSub = interval(5000)
            .pipe(
                startWith(0), // Immediate first run
                switchMap(() => this.missionService.getMissionRequests(this.missionId))
            )
            .subscribe({
                next: (data) => {
                    this.requests.set(data);
                    if (this.isLoading()) {
                        this.isLoading.set(false);
                    }
                    this.cd.detectChanges(); // Ensure UI updates immediately
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

    async approve(request: MissionRequest): Promise<void> {
        const confirmed = await this.notify.confirm(
            'אישור שליח',
            `האם אתה בטוח שברצונך לאשר את השליח ${request.courierName || 'הזה'} למשימה?`,
            'כן, אשר שליח'
        );

        if (!confirmed) return;

        this.isProcessing.set(request.id);
        this.missionService.approveRequest(request.id).subscribe({
            next: () => {
                this.approved.emit();
                this.close.emit();
                this.notify.success('השליח אושר בהצלחה!');
                this.cd.detectChanges();
            },
            error: (err) => {
                console.error(err);
                this.notify.error('שגיאה', 'לא ניתן היה לאשר את הבקשה');
                this.isProcessing.set(null);
                this.cd.detectChanges();
            }
        });
    }

    onBackdropClick(event: MouseEvent) {
        if ((event.target as HTMLElement).classList.contains('custom-modal-wrapper')) {
            this.close.emit();
        }
    }
}
