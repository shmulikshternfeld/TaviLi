import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Mission, PackageSize } from '../../../../core/models/mission.model';
import { PackageSizePipe } from '../../../../shared/pipes/package-size.pipe';

@Component({
    selector: 'app-mission-details-modal',
    standalone: true,
    imports: [CommonModule, PackageSizePipe, RouterModule],
    templateUrl: './mission-details-modal.html',
    styleUrls: ['./mission-details-modal.scss']
})
export class MissionDetailsModalComponent implements OnChanges {
    @Input() mission: Mission | null = null;
    @Input() isRequesting = false;
    @Input() hasAlreadyRequested = false;

    @Output() close = new EventEmitter<void>();
    @Output() request = new EventEmitter<void>();

    private cd = inject(ChangeDetectorRef);

    get isOpen(): boolean {
        return !!this.mission;
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['mission']) {
            // Force UI update when mission changes to ensure content renders immediately
            this.cd.detectChanges();
        }
    }

    onBackdropClick(event: MouseEvent) {
        if ((event.target as HTMLElement).classList.contains('custom-modal-wrapper')) {
            this.close.emit();
        }
    }
}
