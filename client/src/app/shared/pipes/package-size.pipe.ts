import { Pipe, PipeTransform } from '@angular/core';
import { PackageSize } from '../../core/models/mission.model';

@Pipe({
  name: 'packageSize',
  standalone: true
})
export class PackageSizePipe implements PipeTransform {

  transform(value: PackageSize | string | number): string {
    // המרה למספר למקרה שהגיע כסטרינג
    const size = Number(value);

    switch (size) {
      case PackageSize.Small: // 0
        return 'חבילה קטנה ✉️';
      case PackageSize.Medium: // 1
        return 'חבילה בינונית 📦';
      case PackageSize.Large: // 2
        return 'חבילה גדולה 🚚';
      default:
        return 'גודל לא ידוע';
    }
  }
}