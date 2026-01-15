import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { MissionService } from '../../../core/services/mission.service';
import { MapboxService, MapboxFeature } from '../../../core/services/mapbox.service';
import { PackageSize } from '../../../core/models/mission.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-mission-form',
  templateUrl: './mission-form.html',
  styleUrl: './mission-form.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink]
})
export class MissionForm {
  private fb = inject(FormBuilder);
  private missionService = inject(MissionService);
  private router = inject(Router);
  private mapboxService = inject(MapboxService);
  private notificationService = inject(NotificationService);

  form: FormGroup;
  isSubmitting = false;

  pickupSuggestions: MapboxFeature[] = [];
  dropoffSuggestions: MapboxFeature[] = [];
  showPickupSuggestions = false;
  showDropoffSuggestions = false;

  private pickupSubject = new Subject<string>();
  private dropoffSubject = new Subject<string>();

  // Storing full address objects
  selectedPickupAddress: any = null;
  selectedDropoffAddress: any = null;

  packageSizes = [
    { value: PackageSize.Small, label: 'חבילה קטנה (מעטפה / שקית)', shortLabel: 'קטן', icon: '✉️' },
    { value: PackageSize.Medium, label: 'חבילה בינונית (קופסת נעליים)', shortLabel: 'בינוני', icon: '📦' },
    { value: PackageSize.Large, label: 'חבילה גדולה (ארגז / מזוודה)', shortLabel: 'גדול', icon: '🚛' }
  ];

  constructor() {
    this.form = this.fb.group({
      pickupAddress: ['', [Validators.required]],
      pickupEntrance: [''],
      pickupFloor: [''],
      pickupApartment: [''],
      dropoffAddress: ['', [Validators.required]],
      dropoffEntrance: [''],
      dropoffFloor: [''],
      dropoffApartment: [''],
      packageDescription: ['', [Validators.required, Validators.minLength(3)]],
      packageSize: [null, Validators.required],
      offeredPrice: [null, [Validators.required, Validators.min(10)]]
    });

    // Debounced Search for Pickup
    this.pickupSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => this.mapboxService.searchAddress(query))
    ).subscribe(results => {
      this.pickupSuggestions = results;
      this.showPickupSuggestions = true;
    });

    // Debounced Search for Dropoff
    this.dropoffSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => this.mapboxService.searchAddress(query))
    ).subscribe(results => {
      this.dropoffSuggestions = results;
      this.showDropoffSuggestions = true;
    });
  }

  onPickupInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.pickupSubject.next(value);
  }

  onDropoffInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dropoffSubject.next(value);
  }

  selectPickup(feature: MapboxFeature): void {
    this.form.patchValue({ pickupAddress: feature.place_name });
    this.selectedPickupAddress = feature;
    this.showPickupSuggestions = false;
  }

  selectDropoff(feature: MapboxFeature): void {
    this.form.patchValue({ dropoffAddress: feature.place_name });
    this.selectedDropoffAddress = feature;
    this.showDropoffSuggestions = false;
  }

  // Helper to parse Mapbox Feature to our Backend DTO
  private mapFeatureToDto(feature: MapboxFeature, fullAddress: string) {
    if (!feature) return null; // Fallback?

    // Mapbox context: finding city, street etc is tricky without proper parsing types
    // We will extract what we can or rely on place_name
    const city = feature.context?.find(c => c.id.startsWith('place'))?.text;

    return {
      FullAddress: fullAddress,
      Longitude: feature.center[0],
      Latitude: feature.center[1],
      City: city,
      // Street/Number parsing is complex from just 'text', we send the full text mainly
    };
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    if (!this.selectedPickupAddress || !this.selectedDropoffAddress) {
      this.notificationService.warning('כתובת חסרה', 'יש לבחור כתובת מהרשימה');
      return;
    }

    this.isSubmitting = true;

    const basePickup = this.mapFeatureToDto(this.selectedPickupAddress, this.form.value.pickupAddress);
    const baseDropoff = this.mapFeatureToDto(this.selectedDropoffAddress, this.form.value.dropoffAddress);

    const formValue = {
      ...this.form.value,
      packageSize: Number(this.form.value.packageSize),
      pickupAddress: {
        ...basePickup,
        Entrance: this.form.value.pickupEntrance,
        Floor: this.form.value.pickupFloor ? Number(this.form.value.pickupFloor) : null,
        ApartmentNumber: this.form.value.pickupApartment ? Number(this.form.value.pickupApartment) : null
      },
      dropoffAddress: {
        ...baseDropoff,
        Entrance: this.form.value.dropoffEntrance,
        Floor: this.form.value.dropoffFloor ? Number(this.form.value.dropoffFloor) : null,
        ApartmentNumber: this.form.value.dropoffApartment ? Number(this.form.value.dropoffApartment) : null
      }
    };

    this.missionService.createMission(formValue).subscribe({
      next: () => {
        this.router.navigate(['/missions/dashboard']);
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting = false;
        this.notificationService.error('שגיאה', 'אירעה שגיאה ביצירת המשלוח. אנא נסה שנית.');
      }
    });
  }
}
