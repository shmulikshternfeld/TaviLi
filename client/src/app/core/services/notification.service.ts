import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { getToken, onMessage, deleteToken } from '@angular/fire/messaging';
import { Messaging } from '@angular/fire/messaging';
import { catchError, from, Observable, tap, throwError, BehaviorSubject, Subscription, interval, switchMap } from 'rxjs';
import Swal from 'sweetalert2';

export interface NotificationDto {
  id: string;
  title: string;
  body: string;
  actionUrl?: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private messaging = inject(Messaging);
  private router = inject(Router);

  private apiUrl = `${environment.apiUrl}/notifications`;

  // State for Unread Count
  unreadCount = signal<number>(0);

  constructor() {
    this.listenForMessages();
  }

  // --- Push Logic ---

  private pollSub: Subscription | null = null;
  private lastNotificationId: string | null = null;

  startPolling() {
    if (this.pollSub) return;

    this.pollSub = interval(15000).pipe(
      switchMap(() => this.getNotifications(1))
    ).subscribe(res => {
      const list = res.body || [];
      if (list.length > 0) {
        const latest = list[0];
        if (this.lastNotificationId && latest.id !== this.lastNotificationId) {
          if (!latest.isRead) {
            this.showToast(latest);
          }
        }
        this.lastNotificationId = latest.id;
      }
    });
  }

  async initialize() {
    this.startPolling();

    this.getNotifications(1).subscribe(res => {
      const list = res.body || [];
      if (list.length > 0) this.lastNotificationId = list[0].id;
    });

    try {
      const permission = await Notification.requestPermission();
      if (permission === 'granted') {
        const token = await getToken(this.messaging, {
          vapidKey: 'BCjStgjm4l0YTR-edG24xV4NungIWEZIsKl_3a17hKqxY1wFG8Myud1gnKbRDHrYbgjTZl7X3QTuz26Y3yHzqxc'
        });

        if (token) {
          console.log('FCM Token:', token);
          this.subscribe(token).subscribe();
        }
      }
    } catch (err) {
      console.error('Notification permission/token error', err);
    }
  }

  showToast(n: NotificationDto) {
    const { title, body, actionUrl, type } = n;
    Swal.fire({
      title: title,
      text: body,
      icon: (type === 'Success' ? 'success' : 'info') as any,
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: 4000,
      timerProgressBar: true,
      didOpen: (toast) => {
        toast.addEventListener('click', () => {
          if (actionUrl) {
            console.log('Toast clicked (Polling). Navigating to:', actionUrl);
            this.router.navigateByUrl(actionUrl);
          }
        });
      }
    });
  }

  // ... 

  listenForMessages() {
    onMessage(this.messaging, (payload) => {
      console.log('Message received. ', payload);
      this.unreadCount.update(c => c + 1);

      const { title, body } = payload.notification || {};
      const { url, type, actionUrl } = payload.data || {};

      Swal.fire({
        title: title,
        text: body,
        icon: type === 'Success' ? 'success' : 'info',
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true,
        didOpen: (toast) => {
          toast.addEventListener('click', () => {
            const dest = url || actionUrl;
            if (dest) {
              console.log('Toast clicked (Push). Navigating to:', dest);
              this.router.navigateByUrl(dest);
            }
          });
        }
      });
    });
  }
  // ... rest of class code ...

  async logout() {
    try {
      // 1. Delete from FCM
      await deleteToken(this.messaging);

      // 2. We should ideally call unsubscribe API primarily, but currently we just delete locally and let backend handle expiration or explicit unsubscribe if token was stored. 
      // But let's call API if we had the token stored somewhere.
      // For now, simple FCM delete is good start.

      console.log('Logged out from notifications');
    } catch (err) {
      console.error('Logout error', err);
    }
  }

  // --- API Logic ---

  subscribe(token: string) {
    return this.http.post(`${this.apiUrl}/subscribe`, { token, platform: 'Web' });
  }

  unsubscribe(token: string) {
    return this.http.post(`${this.apiUrl}/unsubscribe`, { token });
  }

  getNotifications(page: number = 1) {
    return this.http.get<NotificationDto[]>(`${this.apiUrl}?page=${page}`, { observe: 'response' }).pipe(
      tap(res => {
        const count = res.headers.get('X-Total-Unread');
        if (count) this.unreadCount.set(+count);
      })
    );
  }

  markAsRead(id: string) {
    return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => this.unreadCount.update(c => Math.max(0, c - 1)))
    );
  }

  markAllAsRead() {
    return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCount.set(0))
    );
  }

  // --- UI Helpers (Backward Compatibility) ---

  async confirm(title: string, text: string, confirmButtonText: string = 'כן'): Promise<boolean> {
    const result = await Swal.fire({
      title,
      text,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText,
      cancelButtonText: 'ביטול',
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6'
    });
    return result.isConfirmed;
  }

  info(title: string, text: string = '') {
    Swal.fire({
      title,
      text,
      icon: 'info',
      timer: 2000,
      showConfirmButton: false
    });
  }

  error(title: string, text: string = '') {
    Swal.fire({
      title,
      text,
      icon: 'error',
      confirmButtonText: 'אישור'
    });
  }

  success(title: string, text: string = '') {
    Swal.fire({
      title,
      text,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false
    });
  }

  warning(title: string, text: string = '') {
    Swal.fire({
      title,
      text,
      icon: 'warning',
      confirmButtonText: 'אישור'
    });
  }
}