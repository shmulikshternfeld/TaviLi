import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private http = inject(HttpClient);
  
  // Nominatim Search Endpoint
  private readonly BASE_URL = 'https://nominatim.openstreetmap.org/search';

  constructor() { }

  searchAddress(query: string): Observable<string[]> {
    // Basic validation: wait for at least 3 characters
    if (!query || query.length < 3) return of([]);

    // Nominatim parameters
    // format=json: get JSON response
    // addressdetails=1: get broken down address parts
    // limit=5: only 5 results
    // countrycodes=il: Limit to Israel
    const url = `${this.BASE_URL}?q=${encodeURIComponent(query)}&format=json&addressdetails=1&limit=5&countrycodes=il`;

    return this.http.get<any[]>(url).pipe(
      map(results => {
        // Map the results to a simple array of display names
        return results.map(item => item.display_name);
      }),
      catchError(error => {
        console.error('Nominatim API Error:', error);
        // Return empty array on error so the app doesn't crash
        return of([]);
      })
    );
  }
}