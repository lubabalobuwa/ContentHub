import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, of, switchMap, catchError } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Content } from '../../../../core/models/content.model';
import { UserProfile } from '../../../../core/models/user-profile.model';

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
  me$!: Observable<UserProfile | null>;
  isPublishing = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private contentService: ContentService,
    public auth: AuthService
  ) {
    this.me$ = this.auth.me().pipe(
      catchError(() => of(null))
    );
    this.content$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) return of(null);
        return this.contentService.getById(id);
      })
    );
  }

  publish(content: Content) {
    this.error = null;
    this.isPublishing = true;

    if (!content.rowVersion) {
      this.error = 'Missing row version for publish.';
      this.isPublishing = false;
      return;
    }

    this.contentService.publish(content.id, content.rowVersion).subscribe({
      next: () => {
        // refresh content after publishing
        this.content$ = this.contentService.getById(content.id);
        this.isPublishing = false;
      },
      error: () => {
        this.error = 'Failed to publish content.';
        this.isPublishing = false;
      }
    });
  }
}
