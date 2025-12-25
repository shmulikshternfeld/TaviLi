import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ReviewService } from '../../../core/services/review.service';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-review-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    template: `
    <div class="modal-header border-0 pb-0">
      <h5 class="modal-title fw-bold">דרג את השליח</h5>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <form [formGroup]="reviewForm" (ngSubmit)="onSubmit()">
        
        <div class="text-center mb-4">
          <label class="form-label d-block text-muted mb-2">איך הייתה חווית המשלוח?</label>
          <div class="d-flex justify-content-center gap-2 display-6">
            @for (star of stars; track star) {
              <i class="bi cursor-pointer transition-all" 
                 [class.bi-star-fill]="star <= currentRating"
                 [class.bi-star]="star > currentRating"
                 [class.text-warning]="star <= currentRating"
                 [class.text-secondary]="star > currentRating"
                 (click)="setRating(star)"></i>
            }
          </div>
          @if (reviewForm.get('rating')?.touched && reviewForm.get('rating')?.invalid) {
            <div class="text-danger small mt-2">אנא בחר דירוג (1-5)</div>
          }
        </div>

        <div class="mb-3">
          <label for="comment" class="form-label">הערה (אופציונלי)</label>
          <textarea class="form-control bg-light border-0" id="comment" rows="3" 
                    formControlName="comment" placeholder="מה אהבת? מה אפשר לשפר?"></textarea>
        </div>

        <div class="d-grid">
          <button type="submit" class="btn btn-primary py-2" [disabled]="reviewForm.invalid || isSubmitting">
            @if (isSubmitting) {
              <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            }
            שלח ביקורת
          </button>
        </div>
      </form>
    </div>
  `,
    styles: [`
    .text-warning { color: #FFC107 !important; }
    .cursor-pointer { cursor: pointer; }
    .transition-all { transition: all 0.2s ease; }
    .bi-star-fill:hover { transform: scale(1.1); }
  `]
})
export class ReviewFormComponent {
    activeModal = inject(NgbActiveModal);
    private fb = inject(FormBuilder);
    private reviewService = inject(ReviewService);

    @Input() missionId!: number;
    @Output() reviewSubmitted = new EventEmitter<void>();

    reviewForm: FormGroup;
    stars = [1, 2, 3, 4, 5];
    isSubmitting = false;

    constructor() {
        this.reviewForm = this.fb.group({
            rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
            comment: ['', [Validators.maxLength(500)]]
        });
    }

    get currentRating(): number {
        return this.reviewForm.get('rating')?.value || 0;
    }

    setRating(rating: number) {
        this.reviewForm.patchValue({ rating });
    }

    onSubmit() {
        if (this.reviewForm.invalid) return;

        this.isSubmitting = true;
        const command = {
            missionId: this.missionId,
            ...this.reviewForm.value
        };

        this.reviewService.createReview(command)
            .pipe(finalize(() => this.isSubmitting = false))
            .subscribe({
                next: () => {
                    this.activeModal.close('submitted');
                    this.reviewSubmitted.emit();
                },
                error: (err) => {
                    console.error('Failed to submit review', err);
                    // TODO: Show toast error
                }
            });
    }
}
