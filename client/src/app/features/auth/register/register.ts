import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  styleUrl: './register.scss',
  imports: [ReactiveFormsModule, CommonModule, RouterLink]
})
export class Register {
  registerForm: FormGroup;
  errorMessage: string = '';
  isSubmitting: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      wantsToBeClient: [false],
      wantsToBeCourier: [false]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    // איפוס שגיאות
    this.errorMessage = '';

    // ולידציה עסקית: חובה לבחור לפחות תפקיד אחד
    const { wantsToBeClient, wantsToBeCourier } = this.registerForm.value;
    if (!wantsToBeClient && !wantsToBeCourier) {
      this.errorMessage = 'יש לבחור לפחות תפקיד אחד (שולח או שליח)';
      return;
    }

    this.isSubmitting = true;

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        // הרשמה הצליחה - נווט לדף הבית
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Registration error:', err);
        this.errorMessage = 'הרשמה נכשלה. ייתכן שהאימייל כבר קיים במערכת.';
        this.isSubmitting = false;
      }
    });
  }
}