import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable, map, tap } from 'rxjs';
import { ContentService } from '../../../../../app/core/services/content.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Content } from '../../../../core/models/content.model';
import { PagedResponse } from '../../../../core/models/paged-response.model';

@Component({
  selector: 'app-content-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './content-list.page.html',
  styleUrl: './content-list.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListPage {
  view$!: Observable<{
    paged: PagedResponse<Content>;
    featured: Content | null;
    rest: Content[];
  }>;
  isAuthenticated$!: Observable<boolean>;
  page = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;

  constructor(
    private contentService: ContentService,
    public auth: AuthService
  ) {
    this.isAuthenticated$ = this.auth.authChanges();
    this.load();
  }

  previousPage() {
    if (this.page <= 1) return;
    this.page -= 1;
    this.load();
  }

  nextPage() {
    if (this.page >= this.totalPages) return;
    this.page += 1;
    this.load();
  }

  private load() {
    this.view$ = this.contentService.getPublished(this.page, this.pageSize).pipe(
      tap(response => {
        this.totalPages = response.totalPages || 1;
        this.totalCount = response.totalCount;
      }),
      map(response => ({
        paged: response,
        featured: response.items.length > 0 ? response.items[0] : null,
        rest: response.items.slice(1)
      }))
    );
  }
}
