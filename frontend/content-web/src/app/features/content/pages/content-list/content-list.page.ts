import { AfterViewInit, Component, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable, map, tap } from 'rxjs';
import { ContentService } from '../../../../../app/core/services/content.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Content } from '../../../../core/models/content.model';
import { PagedResponse } from '../../../../core/models/paged-response.model';
import { StripHtmlPipe } from '../../../../core/pipes/strip-html.pipe';

@Component({
  selector: 'app-content-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, StripHtmlPipe],
  templateUrl: './content-list.page.html',
  styleUrl: './content-list.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentListPage implements AfterViewInit {
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
  searchTerm = '';
  newsletterEmail = '';
  newsletterMessage = '';
  newsletterError = '';
  isSubscribing = false;

  constructor(
    private contentService: ContentService,
    public auth: AuthService
  ) {
    this.isAuthenticated$ = this.auth.authChanges();
    this.load();
  }

  ngAfterViewInit() {
    const panel = document.querySelector<HTMLElement>('.newsletter');
    const heading = panel?.querySelector<HTMLElement>('h4');
    const description = panel?.querySelector<HTMLElement>('p:not(.panel-label)');
    const input = panel?.querySelector<HTMLInputElement>('input[type="email"]');
    if (heading) heading.textContent = 'New article alerts';
    if (description) description.textContent = 'Get an email whenever a new story is published.';
    if (input) input.autocomplete = 'email';
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

  search() {
    this.page = 1;
    this.load();
  }

  clearSearch() {
    this.searchTerm = '';
    this.page = 1;
    this.load();
  }

  @HostListener('click', ['$event'])
  handleNewsletterClick(event: MouseEvent) {
    const target = event.target as HTMLElement | null;
    const button = target?.closest<HTMLButtonElement>('.newsletter .cta');
    if (!button) return;

    event.preventDefault();
    const panel = button.closest<HTMLElement>('.newsletter');
    const input = panel?.querySelector<HTMLInputElement>('input[type="email"]');
    this.newsletterEmail = input?.value ?? '';
    this.subscribe();
    this.renderNewsletterState(panel, button, input);
  }

  subscribe() {
    this.newsletterMessage = '';
    this.newsletterError = '';
    const email = this.newsletterEmail.trim();
    if (!email) {
      this.newsletterError = 'Enter your email address.';
      return;
    }

    this.isSubscribing = true;
    this.contentService.subscribeToNewsletter(email).subscribe({
      next: response => {
        this.newsletterMessage = response.message;
        this.newsletterEmail = '';
        this.isSubscribing = false;
        this.renderNewsletterState();
      },
      error: error => {
        this.newsletterError = error?.error?.detail ?? 'Subscription failed. Please try again.';
        this.isSubscribing = false;
        this.renderNewsletterState();
      }
    });
  }

  private renderNewsletterState(
    panel = document.querySelector<HTMLElement>('.newsletter'),
    button = panel?.querySelector<HTMLButtonElement>('.cta'),
    input = panel?.querySelector<HTMLInputElement>('input[type="email"]')
  ) {
    if (!panel || !button) return;

    button.disabled = this.isSubscribing;
    button.textContent = this.isSubscribing ? 'Subscribing...' : 'Subscribe';

    panel.querySelector('.newsletter-status')?.remove();
    const message = this.newsletterError || this.newsletterMessage;
    if (!message) return;

    const status = document.createElement('p');
    status.className = `newsletter-status ${this.newsletterError ? 'newsletter-status--error' : 'newsletter-status--success'}`;
    status.textContent = message;
    status.style.color = this.newsletterError ? '#ffb4a2' : '#9ee6b0';
    status.style.fontSize = '12px';
    status.style.fontWeight = '600';
    status.setAttribute('role', this.newsletterError ? 'alert' : 'status');
    panel.appendChild(status);

    if (this.newsletterMessage && input) input.value = '';
  }

  private load() {
    this.view$ = this.contentService.getPublished(this.page, this.pageSize, this.searchTerm).pipe(
      tap(response => {
        this.totalPages = response.totalPages || 1;
        this.totalCount = response.totalCount;
      }),
      map(response => {
        const items = response.items.map(item => ({
          ...item,
          status: `${this.getReadingTime(item.body)} min read`
        }));

        return {
          paged: response,
          featured: items.length > 0 ? items[0] : null,
          rest: items.slice(1)
        };
      })
    );
  }

  private getReadingTime(body: string): number {
    const wordCount = body
      .replace(/<[^>]*>/g, ' ')
      .replace(/[`*_>#\[\]()~-]/g, ' ')
      .trim()
      .split(/\s+/)
      .filter(Boolean)
      .length;

    return Math.max(1, Math.ceil(wordCount / 225));
  }
}
