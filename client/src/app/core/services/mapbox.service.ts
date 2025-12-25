import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MapboxFeature {
    id: string;
    place_name: string;
    center: [number, number]; // [longitude, latitude]
    text: string; // street name / city name
    context?: any[];
}

export interface MapboxResponse {
    features: MapboxFeature[];
}

@Injectable({
    providedIn: 'root'
})
export class MapboxService {
    private readonly baseUrl = 'https://api.mapbox.com/geocoding/v5/mapbox.places';
    private readonly accessToken = environment.mapboxToken;

    constructor(private http: HttpClient) { }

    searchAddress(query: string): Observable<MapboxFeature[]> {
        if (!query) {
            return new Observable(observer => observer.next([]));
        }

        const url = `${this.baseUrl}/${encodeURIComponent(query)}.json`;
        const params = {
            access_token: this.accessToken,
            autocomplete: 'true',
            language: 'he', // Prefer Hebrew results
            country: 'il',  // Limit to Israel
            limit: '5'
        };

        return this.http.get<MapboxResponse>(url, { params }).pipe(
            map(response => response.features)
        );
    }
}
