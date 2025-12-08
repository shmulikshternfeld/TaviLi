import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { PackageSize } from '../../../core/models/mission.model';
import { AddressService } from '../../../core/services/address.service';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-mission-form',
  templateUrl: './mission-form.html',
  styleUrl: './mission-form.scss',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink]
})
export class MissionForm implements OnInit {
  private fb = inject(FormBuilder);
  private missionService = inject(MissionService);
  private router = inject(Router);
  private addressService = inject(AddressService);

  form: FormGroup;
  isSubmitting = false;

  // Suggestions Arrays to hold API results
  pickupSuggestions: string[] = [];
  dropoffSuggestions: string[] = [];
  
  // Search Streams to handle typing events
  private pickupSearch$ = new Subject<string>();
  private dropoffSearch$ = new Subject<string>();

  packageSizes = [
    { value: PackageSize.Small, label: 'חבילה קטנה (מעטפה / שקית)' },
    { value: PackageSize.Medium, label: 'חבילה בינונית (קופסת נעליים)' },
    { value: PackageSize.Large, label: 'חבילה גדולה (ארגז / מזוודה)' }
  ];

  constructor() {
    this.form = this.fb.group({
      pickupAddress: ['', [Validators.required, Validators.minLength(5)]],
      dropoffAddress: ['', [Validators.required, Validators.minLength(5)]],
      packageDescription: ['', [Validators.required, Validators.minLength(3)]],
      packageSize: [null, Validators.required],
      offeredPrice: [null, [Validators.required, Validators.min(10)]]
    });
  }

  ngOnInit(): void {
    // --- Pickup Autocomplete Logic ---
    this.pickupSearch$.pipe(
      debounceTime(300), // Wait 300ms after typing stops
      distinctUntilChanged(), // Only search if value changed
      switchMap(term => this.addressService.searchAddress(term)) // Call API
    ).subscribe(results => this.pickupSuggestions = results);

    // --- Dropoff Autocomplete Logic ---
    this.dropoffSearch$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => this.addressService.searchAddress(term))
    ).subscribe(results => this.dropoffSuggestions = results);
  }

  // --- Input Handlers (Triggered by HTML) ---
  onPickupInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.pickupSearch$.next(value);
  }

  onDropoffInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dropoffSearch$.next(value);
  }

  // --- Selection Handlers (Triggered by clicking a suggestion) ---
  selectPickup(address: string): void {
    this.form.patchValue({ pickupAddress: address });
    this.pickupSuggestions = []; // Hide the list
  }

  selectDropoff(address: string): void {
    this.form.patchValue({ dropoffAddress: address });
    this.dropoffSuggestions = []; // Hide the list
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isSubmitting = true;
    
    const formValue = {
      ...this.form.value,
      packageSize: Number(this.form.value.packageSize),
      offeredPrice: Number(this.form.value.offeredPrice)
    };

    this.missionService.createMission(formValue).subscribe({
      next: () => {
        this.router.navigate(['/missions/dashboard']);
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting = false;
      }
    });
  }
}