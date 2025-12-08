import { Component, inject, signal, effect, OnInit } from '@angular/core';
import { ImageCropperComponent, ImageCroppedEvent, LoadedImage } from 'ngx-image-cropper';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ImageCompressionService } from '../../../core/services/image-compression.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserProfile } from '../../../core/models/user.model';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ImageCropperComponent],
    templateUrl: './profile.html',
    styleUrls: ['./profile.scss']
})
export class ProfileComponent implements OnInit {
    private authService = inject(AuthService);
    private imageService = inject(ImageCompressionService);
    private notify = inject(NotificationService);
    private fb = inject(FormBuilder);
    private route = inject(ActivatedRoute);

    // Signals
    viewedUser = signal<UserProfile | null>(null); // The user being displayed (could be me or someone else)
    isReadOnly = signal(false); // True if viewing someone else's profile

    isEditing = signal(false);
    isSaving = signal(false);

    // Cropper state
    isCropping = signal(false);
    imageChangedEvent: any = '';
    croppedImage: Blob | null = null;

    // Image preview state
    imagePreview = signal<string | null>(null);
    selectedFile: File | null = null;

    form = this.fb.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        phoneNumber: ['', [Validators.pattern('^[0-9+ -]*$')]] // Basic phone validation
    });

    constructor() {
        effect(() => {
            const user = this.viewedUser();
            if (user) {
                this.form.patchValue({
                    name: user.name,
                    email: user.email,
                    phoneNumber: user.phoneNumber
                });
                this.imagePreview.set(user.profileImageUrl || null);
            }
        });
    }

    ngOnInit() {
        this.route.paramMap.subscribe(params => {
            const userId = params.get('id');
            if (userId) {
                // Public Profile View
                this.isReadOnly.set(true);
                this.authService.getUserById(userId).subscribe({
                    next: (user) => this.viewedUser.set(user),
                    error: () => this.notify.error('שגיאה', 'לא ניתן לטעון פרופיל משתמש')
                });
            } else {
                // My Profile View
                this.isReadOnly.set(false);
                // Effect will handle loading from authService.currentUser() but let's be explicit
                // Use a computed if we want reactivity, or just subscribe to currentUser
                // Better: link viewedUser to authService.currentUser when not read-only
                this.viewedUser.set(this.authService.currentUser());
            }
        });
    }

    toggleEdit() {
        if (this.isReadOnly()) return;

        this.isEditing.update(v => !v);
        if (!this.isEditing()) {
            // Reset if cancelled
            const user = this.viewedUser();
            if (user) {
                this.form.patchValue({
                    name: user.name,
                    email: user.email,
                    phoneNumber: user.phoneNumber
                });
                this.imagePreview.set(user.profileImageUrl || null);
                this.selectedFile = null;
                this.cancelCrop();
            }
        }
    }

    onFileSelected(event: Event) {
        if (this.isReadOnly()) return;
        const input = event.target as HTMLInputElement;
        if (!input.files?.length) return;

        this.imageChangedEvent = event;
        this.isCropping.set(true);
    }

    imageCropped(event: ImageCroppedEvent) {
        this.croppedImage = event.blob || null;
    }

    imageLoaded(image: LoadedImage) {
        // show cropper
    }

    cropperReady() {
        // cropper ready
    }

    loadImageFailed() {
        this.notify.error('שגיאה', 'טעינת התמונה נכשלה');
        this.cancelCrop();
    }

    async confirmCrop() {
        if (!this.croppedImage) return;

        try {
            this.notify.info('מעבד תמונה...');
            const file = new File([this.croppedImage], "profile_cropped.jpg", { type: "image/jpeg" });
            const compressedFile = await this.imageService.compressImage(file);
            this.selectedFile = compressedFile;

            const reader = new FileReader();
            reader.onload = (e: any) => this.imagePreview.set(e.target.result);
            reader.readAsDataURL(compressedFile);

            this.cancelCrop();
        } catch (err) {
            console.error(err);
            this.notify.error('שגיאה', 'עיבוד התמונה נכשל');
        }
    }

    cancelCrop() {
        this.isCropping.set(false);
        this.imageChangedEvent = '';
        this.croppedImage = null;
    }

    save() {
        if (this.form.invalid) return;

        this.isSaving.set(true);
        const name = this.form.value.name!;
        const email = this.form.value.email!;
        const phone = this.form.value.phoneNumber || undefined;

        this.authService.updateProfile(name, email, phone, this.selectedFile || undefined).subscribe({
            next: (updatedUser) => {
                this.notify.success('פרופיל עודכן בהצלחה');
                this.viewedUser.set(updatedUser); // Update local view
                this.isEditing.set(false);
                this.selectedFile = null;
                this.isSaving.set(false);
            },
            error: (err) => {
                console.error('Profile update failed:', err);
                if (err.error) {
                    console.error('Server error details:', JSON.stringify(err.error));
                    this.notify.error('שגיאה', typeof err.error === 'string' ? err.error : 'עדכון הפרופיל נכשל');
                } else {
                    this.notify.error('שגיאה', 'עדכון הפרופיל נכשל');
                }
                this.isSaving.set(false);
            }
        });
    }
}
