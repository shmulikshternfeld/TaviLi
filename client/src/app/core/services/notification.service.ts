import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  // צבעי המותג שלנו
  private readonly colors = {
    confirm: '#00B894', // Teal
    cancel: '#FF7675'   // Coral
  };

  // הודעת הצלחה (Toast קטן בצד)
  success(message: string): void {
    Swal.fire({
      icon: 'success',
      title: message,
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true
    });
  }

  // הודעת שגיאה (Modal במרכז)
  error(title: string, text?: string): void {
    Swal.fire({
      icon: 'error',
      title: title,
      text: text,
      confirmButtonColor: this.colors.cancel,
      confirmButtonText: 'סגור'
    });
  }

  // הודעת אישור (Confirm)
  async confirm(title: string, text: string, confirmBtnText = 'כן, אני בטוח'): Promise<boolean> {
    const result = await Swal.fire({
      title: title,
      text: text,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: this.colors.confirm,
      cancelButtonColor: this.colors.cancel,
      confirmButtonText: confirmBtnText,
      cancelButtonText: 'ביטול',
      reverseButtons: true // כפתור אישור בצד ימין (בעברית)
    });

    return result.isConfirmed;
  }
}