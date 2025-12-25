import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { Observable } from 'rxjs';

export interface CreateReviewCommand {
    missionId: number;
    rating: number;
    comment?: string;
}

@Injectable({
    providedIn: 'root'
})
export class ReviewService {
    private http = inject(HttpClient);
    private apiUrl = environment.apiUrl + '/reviews';

    createReview(command: CreateReviewCommand): Observable<number> {
        return this.http.post<number>(this.apiUrl, command);
    }
}
