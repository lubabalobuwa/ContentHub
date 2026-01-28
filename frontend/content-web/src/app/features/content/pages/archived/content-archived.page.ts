import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EMPTY, Observable, of, switchMap, tap, timeout, catchError, finalize } from 'rxjs';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ContentService } from '../../../../core/services/content.service';
import { Content } from '../../../../core/models/content.model';
import { PagedResponse } from '../../../../core/models/paged-response.model';

@Component({
  selector: 'app-content-archived-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './content-archived.page.html',
  styleUrl: './content-archived.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentArchivedPage {
  archived$!: Observable<PagedResponse<Content>>;
  page = 1;
  pageSize = 10;
  totalPages = 1;
  error: string | null = null;
  pending: Record<string, 'restore'> = {};

  constructor(
    private auth: AuthService,
    private contentService: ContentService,
    private cdr: ChangeDetectorRef
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

  restore(item: Content) {
    this.error = null;
    if (!item.rowVersion) {
      this.error = 'Missing row version.';
      return;
    }

    this.pending[item.id] = 'restore';
    this.cdr.markForCheck();
    this.contentService.restore(item.id, item.rowVersion).pipe(
      timeout(15000),
      tap(() => this.load()),
      catchError(() => {
        this.error = 'Failed to restore archived content.';
        return EMPTY;
      }),
      finalize(() => {
        delete this.pending[item.id];
        this.cdr.markForCheck();
      })
    ).subscribe();
  }

  isBusy(item: Content) {
    return !!this.pending[item.id];
  }

  private load() {
    this.archived$ = this.auth.me().pipe(
      switchMap(profile => {
        if (!profile?.id) return of({ items: [], page: 1, pageSize: 1, totalCount: 0, totalPages: 1 });
        return this.contentService.getArchivedByAuthor(profile.id, this.page, this.pageSize);
      }),
      tap(response => this.totalPages = response.totalPages || 1)
    );
  }
}
