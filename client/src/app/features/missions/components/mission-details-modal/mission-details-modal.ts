import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Mission, PackageSize } from '../../../../core/models/mission.model';
import { PackageSizePipe } from '../../../../shared/pipes/package-size.pipe';

@Component({
    selector: 'app-mission-details-modal',
    standalone: true,
    imports: [CommonModule, PackageSizePipe],
    templateUrl: './mission-details-modal.html',
    styleUrls: ['./mission-details-modal.scss']
})
export class MissionDetailsModalComponent {
    @Input() mission: Mission | null = null;
    @Input() isRequesting = false;
    @Input() hasAlreadyRequested = false;

    @Output() close = new EventEmitter<void>();
    @Output() request = new EventEmitter<void>();

    get isOpen(): boolean {
        return !!this.mission;
    }

    onBackdropClick(event: MouseEvent) {
        if ((event.target as HTMLElement).classList.contains('custom-modal-wrapper')) {
            this.close.emit();
        }
    }
}
