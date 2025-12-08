import { Component, EventEmitter, Input, Output, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MissionService } from '../../../../core/services/mission.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MissionRequest, MissionRequestStatus } from '../../../../core/models/mission.model';

@Component({
    selector: 'app-mission-requests-modal',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './mission-requests-modal.html',
    styleUrls: ['./mission-requests-modal.scss']
})
export class MissionRequestsModalComponent implements OnInit {
    @Input() missionId!: number;
    @Output() close = new EventEmitter<void>();
    @Output() approved = new EventEmitter<void>(); // Emit when a request is approved

    private missionService = inject(MissionService);
    private notify = inject(NotificationService);

    requests = signal<MissionRequest[]>([]);
    isLoading = signal<boolean>(true);
    isProcessing = signal<number | null>(null);

    ngOnInit(): void {
        this.loadRequests();
    }

    loadRequests(): void {
        this.isLoading.set(true);
        this.missionService.getMissionRequests(this.missionId).subscribe({
            next: (data) => {
                this.requests.set(data);
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error(err);
                this.isLoading.set(false);
            }
        });
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
            },
            error: (err) => {
                console.error(err);
                this.notify.error('שגיאה', 'לא ניתן היה לאשר את הבקשה');
                this.isProcessing.set(null);
            }
        });
    }

    onBackdropClick(event: MouseEvent) {
        if ((event.target as HTMLElement).classList.contains('custom-modal-wrapper')) {
            this.close.emit();
        }
    }
}
