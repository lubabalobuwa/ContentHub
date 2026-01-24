import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Content } from '../models/content.model';
import { environment } from '../../../environments/environments';

@Injectable({ providedIn: 'root' })
export class ContentService {
    private readonly baseUrl = `${environment.apiBaseUrl}/content`;

    constructor(private http: HttpClient) {}

    getPublished(): Observable<Content[]> {
        return this.http.get<Content[]>(this.baseUrl);
    }

    getById(id: string): Observable<Content> {
        return this.http.get<Content>(`${this.baseUrl}/${id}`);
    }

    create(payload: { title: string; body: string; authorId: string }) {
        return this.http.post(this.baseUrl, payload);
    }

    publish(id: string) {
        return this.http.post(`${this.baseUrl}/${id}/publish`, {});
    }
}