import { Component, inject, signal, ElementRef, HostListener } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive]
})
export class Navbar {
  authService = inject(AuthService);
  public router = inject(Router);
  private notify = inject(NotificationService);
  private eRef = inject(ElementRef);

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (this.isUserMenuOpen()) {
      const target = event.target as HTMLElement;
      // Check if click is inside the user dropdown container
      const container = this.eRef.nativeElement.querySelector('.user-dropdown-container');
      const menu = this.eRef.nativeElement.querySelector('.user-dropdown-menu');

      if (container && container.contains(target)) {
        return; // Clicked on trigger
      }
      if (menu && menu.contains(target)) {
        return; // Clicked inside menu
      }

      // Clicked outside
      this.closeUserMenu();
    }
  }

  // Responsive Menu State
  isMenuOpen = signal<boolean>(false);
  isUserMenuOpen = signal<boolean>(false);

  toggleMenu() {
    this.isMenuOpen.update(v => !v);
  }

  closeMenu() {
    this.isMenuOpen.set(false);
  }

  toggleUserMenu() {
    this.isUserMenuOpen.update(v => !v);
  }

  closeUserMenu() {
    this.isUserMenuOpen.set(false);
  }

  async logoutWithConfirmation() {
    this.closeUserMenu();
    this.closeMenu();

    const confirmed = await this.notify.confirm(
      'התנתקות',
      'האם אתה בטוח שברצונך להתנתק?',
      'כן, התנתק'
    );

    if (confirmed) {
      this.authService.logout();
      this.router.navigate(['/auth/login']);
      this.notify.info('התנתקת בהצלחה');
    }
  }
  // Touch / Swipe Logic for Mobile Drawer
  private touchStartX = 0;
  private touchEndX = 0;

  onTouchStart(event: TouchEvent) {
    this.touchStartX = event.changedTouches[0].screenX;
  }

  onTouchEnd(event: TouchEvent) {
    this.touchEndX = event.changedTouches[0].screenX;
    this.handleSwipe();
  }

  private handleSwipe() {
    const threshold = 50; // Minimum distance to be considered a swipe
    // Swipe Right (Left -> Right): endX > startX
    // Since the drawer is on the Right, swiping Right pushes it "away" (closes it).
    if (this.touchEndX - this.touchStartX > threshold) {
      this.closeMenu();
    }
  }
}
