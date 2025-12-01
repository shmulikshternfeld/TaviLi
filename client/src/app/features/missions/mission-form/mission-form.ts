import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MissionService } from '../../../core/services/mission.service';
import { PackageSize } from '../../../core/models/mission.model';

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

  form: FormGroup;
  isSubmitting = false;

  // המרה של ה-Enum למערך כדי להציג ב-Select
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
      packageSize: [null, Validators.required], // חובה לבחור
      offeredPrice: [null, [Validators.required, Validators.min(10)]] // מינימום 10 ש"ח
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isSubmitting = true;
    
    // המרה למספר (למקרה שה-Select מחזיר סטרינג)
    const formValue = {
      ...this.form.value,
      packageSize: Number(this.form.value.packageSize)
    };

    this.missionService.createMission(formValue).subscribe({
      next: () => {
        // הצלחה - חזרה לדשבורד
        this.router.navigate(['/missions/dashboard']);
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting = false;
        // כאן נוסיף בעתיד Toast שגיאה
      }
    });
  }
}
