import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private http = inject(HttpClient);

  // Geoapify Autocomplete Endpoint
  private readonly BASE_URL = 'https://api.geoapify.com/v1/geocode/autocomplete';

  constructor() { }

  searchAddress(query: string): Observable<string[]> {
    if (!query || query.length < 3) return of([]);

    // Use the key from environment or fallback (though it won't work without a valid key)
    // @ts-ignore
    const apiKey = environment.geoapifyApiKey || '';

    if (!apiKey) {
      console.warn('Geoapify API Key is missing in environment.ts');
      return of([]);
    }

    // Geoapify parameters
    // text: the query
    // apiKey: your key
    // limit: number of results
    // format: json
    const url = `${this.BASE_URL}?text=${encodeURIComponent(query)}&limit=5&format=json&apiKey=${apiKey}`;

    return this.http.get<any>(url).pipe(
      map(response => {
        // Geoapify returns { results: [...] }
        if (!response.results) return [];

        // Map to formatted address
        return response.results.map((item: any) => item.formatted);
      }),
      catchError(error => {
        console.error('Geoapify API Error:', error);
        return of([]);
      })
    );
  }
}
