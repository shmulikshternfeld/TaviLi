import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PackageSize } from '../../../../core/models/mission.model';

export interface FilterState {
    relatedCity: string;
    pickupCity: string;
    dropoffCity: string;
    packageSize: PackageSize | 'All';
}

@Component({
    selector: 'app-mission-filter',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './mission-filter.html',
    styleUrls: ['./mission-filter.scss']
})
export class MissionFilterComponent {
    @Output() filterChange = new EventEmitter<FilterState>();

    relatedCity: string = '';
    pickupCity: string = '';
    dropoffCity: string = '';
    packageSize: PackageSize | 'All' = 'All';

    // מאפשר להשתמש ב-Enum ב-Template
    PackageSize = PackageSize;

    // מיפוי גדלים לתצוגה בעברית עם אימוג'י
    sizeOptions = [
        { value: PackageSize.Small, label: 'קטן ✉️' },
        { value: PackageSize.Medium, label: 'בינוני 📦' },
        { value: PackageSize.Large, label: 'גדול 🚛' }
    ];

    onFilterChange() {
        this.filterChange.emit({
            relatedCity: this.relatedCity,
            pickupCity: this.pickupCity,
            dropoffCity: this.dropoffCity,
            packageSize: this.packageSize
        });
    }

    setPackageSize(size: PackageSize | 'All') {
        this.packageSize = size;
        this.onFilterChange();
    }
}
