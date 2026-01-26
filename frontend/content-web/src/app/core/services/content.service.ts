import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Content } from '../models/content.model';
import { environment } from '../../../environments/environments';
import { PagedResponse } from '../models/paged-response.model';

@Injectable({ providedIn: 'root' })
export class ContentService {
    private readonly baseUrl = `${environment.apiBaseUrl}/content`;

    constructor(private http: HttpClient) {}

    getPublished(page = 1, pageSize = 20): Observable<PagedResponse<Content>> {
        const params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);

        return this.http.get<PagedResponse<Content>>(this.baseUrl, { params });
    }

    getById(id: string): Observable<Content> {
        return this.http.get<Content>(`${this.baseUrl}/${id}`);
    }

    create(payload: { title: string; body: string }) {
        return this.http.post(this.baseUrl, payload);
    }

    publish(id: string) {
        return this.http.post(`${this.baseUrl}/${id}/publish`, {});
    }
}
