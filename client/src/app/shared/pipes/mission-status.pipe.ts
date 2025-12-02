import { Pipe, PipeTransform } from '@angular/core';
import { MissionStatus } from '../../core/models/mission.model';

@Pipe({
  name: 'missionStatus',
  standalone: true
})
export class MissionStatusPipe implements PipeTransform {

  transform(value: MissionStatus | number | string): string {
    const status = Number(value);

    switch (status) {
      case MissionStatus.Open:
        return 'ממתין לשליח';
      case MissionStatus.Accepted:
        return 'שליח משויך (התקבל)';
      case MissionStatus.InProgress_Pickup:
        return 'בדרך לאיסוף 🛵';
      case MissionStatus.Collected:
        return 'נאסף (אצל השליח) 📦';
      case MissionStatus.InProgress_Delivery:
        return 'בדרך למסירה 🏁';
      case MissionStatus.Completed:
        return 'הושלם בהצלחה ✅';
      default:
        return 'סטטוס לא ידוע';
    }
  }
}