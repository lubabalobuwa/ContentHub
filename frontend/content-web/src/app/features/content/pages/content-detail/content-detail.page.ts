import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { Content } from '../../../../core/models/content.model';

@Component({
  selector: 'app-content-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './content-detail.page.html',
  styleUrl: './content-detail.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentDetailPage {
  content$!: Observable<Content | null>;
  isPublishing = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private contentService: ContentService
  ) {
    this.content$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) return of(null);
        return this.contentService.getById(id);
      })
    );
  }

  publish(id: string) {
    this.error = null;
    this.isPublishing = true;

    this.contentService.publish(id).subscribe({
      next: () => {
        // refresh content after publishing
        this.content$ = this.contentService.getById(id);
        this.isPublishing = false;
      },
      error: () => {
        this.error = 'Failed to publish content.';
        this.isPublishing = false;
      }
    });
  }
}
