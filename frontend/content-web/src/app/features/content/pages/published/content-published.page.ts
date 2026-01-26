import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of, switchMap, tap } from 'rxjs';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ContentService } from '../../../../core/services/content.service';
import { Content } from '../../../../core/models/content.model';
import { PagedResponse } from '../../../../core/models/paged-response.model';

@Component({
  selector: 'app-content-published-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './content-published.page.html',
  styleUrl: './content-published.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentPublishedPage {
  published$!: Observable<PagedResponse<Content>>;
  page = 1;
  pageSize = 10;
  totalPages = 1;
  error: string | null = null;
  pending: Record<string, 'archive'> = {};

  constructor(
    private auth: AuthService,
    private contentService: ContentService
  ) {
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

  archive(item: Content) {
    this.error = null;
    if (!item.rowVersion) {
      this.error = 'Missing row version.';
      return;
    }

    this.pending[item.id] = 'archive';
    this.contentService.archive(item.id, item.rowVersion).subscribe({
      next: () => {
        delete this.pending[item.id];
        this.load();
      },
      error: () => {
        delete this.pending[item.id];
        this.error = 'Failed to archive content.';
      }
    });
  }

  isBusy(item: Content) {
    return !!this.pending[item.id];
  }

  private load() {
    this.published$ = this.auth.me().pipe(
      switchMap(profile => {
        if (!profile?.id) return of({ items: [], page: 1, pageSize: 1, totalCount: 0, totalPages: 1 });
        return this.contentService.getPublishedByAuthor(profile.id, this.page, this.pageSize);
      }),
      tap(response => this.totalPages = response.totalPages || 1)
    );
  }
}
