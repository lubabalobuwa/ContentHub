import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { ContentService } from '../../../../../app/core/services/content.service';
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
  contents$!: Observable<PagedResponse<Content>>;
  page = 1;
  pageSize = 10;
  totalPages = 1;
  totalCount = 0;

  constructor(private contentService: ContentService) {
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
    this.contents$ = this.contentService.getPublished(this.page, this.pageSize).pipe(
      tap(response => {
        this.totalPages = response.totalPages || 1;
        this.totalCount = response.totalCount;
      })
    );
  }
}
