import { Component, ChangeDetectionStrategy, ElementRef, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, of, switchMap, catchError } from 'rxjs';
import { ContentService } from '../../../../core/services/content.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Content } from '../../../../core/models/content.model';
import { UserProfile } from '../../../../core/models/user-profile.model';
import { SafeHtmlPipe } from '../../../../core/pipes/safe-html.pipe';
import { CommentService } from '../../../../core/services/comment.service';
import { Comment } from '../../../../core/models/comment.model';
import hljs from 'highlight.js/lib/common';

@Component({
  selector: 'app-content-detail-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SafeHtmlPipe],
  templateUrl: './content-detail.page.html',
  styleUrl: './content-detail.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentDetailPage implements AfterViewInit, OnDestroy {
  @ViewChild('contentBody', { static: false }) contentBodyRef?: ElementRef<HTMLElement>;

  content$!: Observable<Content | null>;
  comments$!: Observable<Comment[]>;
  me$!: Observable<UserProfile | null>;
  isPublishing = false;
  error: string | null = null;
  commentError: string | null = null;
  isSubmittingComment = false;
  newComment = '';
  private contentId: string | null = null;
  private readonly commentPreviewLimit = 260;
  expandedComments = new Set<string>();
  readonly commentDisplayLimit = 3;
  showAllComments = false;
  private lastHighlightedId: string | null = null;
  private observer?: MutationObserver;

  constructor(
    private route: ActivatedRoute,
    private contentService: ContentService,
    private commentService: CommentService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    this.me$ = this.auth.me().pipe(
      catchError(() => of(null))
    );
    this.content$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        this.contentId = id;
        if (id) {
          this.loadComments(id);
        }
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
    this.comments$ = of([]);
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

  submitComment() {
    if (!this.contentId) return;
    const text = this.newComment.trim();
    if (!text) {
      this.commentError = 'Comment text is required.';
      return;
    }

    this.commentError = null;
    this.isSubmittingComment = true;

    this.commentService.create(this.contentId, text).subscribe({
      next: () => {
        this.newComment = '';
        this.isSubmittingComment = false;
        this.cdr.markForCheck();
        this.loadComments(this.contentId!);
      },
      error: () => {
        this.isSubmittingComment = false;
        this.commentError = 'Failed to post comment.';
        this.cdr.markForCheck();
      }
    });
  }

  toggleComment(commentId: string) {
    if (this.expandedComments.has(commentId)) {
      this.expandedComments.delete(commentId);
    } else {
      this.expandedComments.add(commentId);
    }
    this.cdr.markForCheck();
  }

  isCommentExpanded(commentId: string): boolean {
    return this.expandedComments.has(commentId);
  }

  getCommentPreview(text: string): string {
    if (text.length <= this.commentPreviewLimit) return text;
    return text.slice(0, this.commentPreviewLimit).trimEnd();
  }

  isCommentTruncated(text: string): boolean {
    return text.length > this.commentPreviewLimit;
  }

  getVisibleComments(comments: Comment[]): Comment[] {
    if (this.showAllComments) return comments;
    return comments.slice(0, this.commentDisplayLimit);
  }

  hasHiddenComments(comments: Comment[]): boolean {
    return comments.length > this.commentDisplayLimit;
  }

  toggleAllComments() {
    this.showAllComments = !this.showAllComments;
    this.cdr.markForCheck();
  }

  private applyHighlighting() {
    const host = this.contentBodyRef?.nativeElement;
    if (!host) return;

    const wrappers = host.querySelectorAll('.code-block-wrapper');
    wrappers.forEach((wrapper: Element) => {
      const codeEl = wrapper.querySelector('code');
      if (!codeEl || (wrapper as HTMLElement).dataset['highlighted'] === 'true') return;

      const language = (wrapper as HTMLElement).dataset['language'] || codeEl.className.replace('language-', '').trim();
      const source = codeEl.textContent ?? '';
      if (!source) return;

      try {
        const normalizedLanguage = language || 'plaintext';
        const result = hljs.highlight(source, { language: normalizedLanguage }).value;
        codeEl.innerHTML = result;
        codeEl.classList.add('hljs');
      } catch {
        codeEl.innerHTML = source;
      }

      (wrapper as HTMLElement).dataset['highlighted'] = 'true';

      const button = wrapper.querySelector('.copy-code-btn') as HTMLButtonElement | null;
      if (!button) return;

      button.addEventListener('click', async () => {
        try {
          await navigator.clipboard.writeText(codeEl.textContent ?? '');
          const previous = button.innerHTML;
          button.classList.add('copied');
          button.title = 'Copied';
          button.innerHTML = '<svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M9.55 15.15 4.7 10.3l1.4-1.4 3.45 3.45 8.85-8.85 1.4 1.4-10.25 10.25Z" fill="currentColor"/></svg>';
          setTimeout(() => {
            button.classList.remove('copied');
            button.title = 'Copy code';
            button.innerHTML = previous;
          }, 1400);
        } catch {
          button.title = 'Copy failed';
        }
      }, { once: true });
    });
  }

  private loadComments(contentId: string) {
    this.comments$ = this.commentService.getByContentId(contentId).pipe(
      catchError(() => {
        this.commentError = 'Failed to load comments.';
        this.cdr.markForCheck();
        return of([]);
      })
    );
  }
}
