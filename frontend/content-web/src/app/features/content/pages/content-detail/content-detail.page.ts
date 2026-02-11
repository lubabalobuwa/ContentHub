import { Component, ChangeDetectionStrategy, ElementRef, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, of, switchMap, catchError } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Content } from '../../../../core/models/content.model';
import { UserProfile } from '../../../../core/models/user-profile.model';
import { SafeHtmlPipe } from '../../../../core/pipes/safe-html.pipe';
import hljs from 'highlight.js/lib/common';

@Component({
  selector: 'app-content-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, SafeHtmlPipe],
  templateUrl: './content-detail.page.html',
  styleUrl: './content-detail.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentDetailPage implements AfterViewInit, OnDestroy {
  @ViewChild('contentBody', { static: false }) contentBodyRef?: ElementRef<HTMLElement>;

  content$!: Observable<Content | null>;
  me$!: Observable<UserProfile | null>;
  isPublishing = false;
  error: string | null = null;
  private lastHighlightedId: string | null = null;
  private observer?: MutationObserver;

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
    this.content$.subscribe(content => {
      if (!content) return;
      if (this.lastHighlightedId === content.id) return;
      this.lastHighlightedId = content.id;
      setTimeout(() => this.applyHighlighting(), 0);
    });
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

  ngAfterViewInit() {
    const host = this.contentBodyRef?.nativeElement;
    if (!host) return;

    this.observer = new MutationObserver(() => this.applyHighlighting());
    this.observer.observe(host, { childList: true, subtree: true });
    setTimeout(() => this.applyHighlighting(), 0);
  }

  ngOnDestroy() {
    this.observer?.disconnect();
    this.observer = undefined;
  }

  private applyHighlighting() {
    const host = this.contentBodyRef?.nativeElement;
    if (!host) return;

    const containers = host.querySelectorAll('.ql-code-block-container');
    containers.forEach(container => {
      if ((container as HTMLElement).dataset['highlighted'] === 'true') return;
      const lines = Array.from(container.querySelectorAll('.ql-code-block'))
        .map(line => line.textContent ?? '');
      const codeText = lines.join('\n');
      const highlighted = hljs.highlight(codeText, { language: 'csharp' }).value;

      const pre = document.createElement('pre');
      const code = document.createElement('code');
      code.className = 'language-csharp';
      code.innerHTML = highlighted;
      pre.appendChild(code);

      (container as HTMLElement).dataset['highlighted'] = 'true';
      container.replaceWith(pre);
    });
  }
}
