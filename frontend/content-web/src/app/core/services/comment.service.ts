import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';
import { Comment } from '../models/comment.model';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/content`;

  constructor(private http: HttpClient) {}

  getByContentId(contentId: string): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/${contentId}/comments`);
  }

  create(contentId: string, text: string): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/${contentId}/comments`, { text });
  }
}
