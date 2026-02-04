import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environments';
import { PagedResponse } from '../models/paged-response.model';
import { AdminUser } from '../models/admin-user.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly baseUrl = `${environment.apiBaseUrl}/admin`;

  constructor(private http: HttpClient) {}

  getUsers(page = 1, pageSize = 20, search = ''): Observable<PagedResponse<AdminUser>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim().length > 0) {
      params = params.set('search', search.trim());
    }

    return this.http.get<PagedResponse<AdminUser>>(`${this.baseUrl}/users`, { params });
  }

  setUserDisabled(id: string, isDisabled: boolean) {
    return this.http.put(`${this.baseUrl}/users/${id}/status`, { isDisabled });
  }
}
