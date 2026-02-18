import { Component, ChangeDetectionStrategy, AfterViewInit, OnDestroy, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../../environments/environments';
import { HttpClient } from '@angular/common/http';
import { catchError, finalize, throwError, timeout } from 'rxjs';

@Component({
  selector: 'app-contact-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.page.html',
  styleUrl: './contact.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactPage implements AfterViewInit, OnDestroy {
  name = '';
  email = '';
  topic = '';
  message = '';
  error: string | null = null;
  success: string | null = null;
  private successTimeoutId: number | null = null;
  isSubmitting = false;
  readonly turnstileSiteKey = environment.turnstileSiteKey;
  private turnstileWidgetId: string | null = null;
  turnstileToken = '';

  @ViewChild('turnstileContainer', { static: false }) turnstileContainer?: ElementRef<HTMLDivElement>;

  private readonly requestTimeoutMs = 15000;

  constructor(
    private cdr: ChangeDetectorRef,
    private http: HttpClient
  ) {}

  ngAfterViewInit() {
    if (!this.turnstileSiteKey) {
      return;
    }

    this.loadTurnstileScript()
      .then(() => this.renderTurnstile())
      .catch(() => {
        this.error = 'Captcha failed to load. Please refresh.';
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy() {
    const turnstile = (window as any).turnstile;
    if (turnstile && this.turnstileWidgetId) {
      turnstile.remove(this.turnstileWidgetId);
    }
  }

  submit() {
    this.error = null;
    this.success = null;
    if (this.successTimeoutId) {
      window.clearTimeout(this.successTimeoutId);
      this.successTimeoutId = null;
    }
    this.syncTurnstileToken();

    if (!this.turnstileToken) {
      this.error = 'Please complete the captcha check.';
      this.cdr.markForCheck();
      return;
    }

    if (!this.message.trim()) {
      this.error = 'Please add a message before sending.';
      this.cdr.markForCheck();
      return;
    }

    this.isSubmitting = true;
    const subject = this.topic?.trim() || 'Support request';
    this.http.post<{ message: string; warning?: string }>(`${environment.apiBaseUrl}/support/contact`, {
      name: this.name.trim(),
      email: this.email.trim(),
      topic: subject,
      message: this.message.trim(),
      turnstileToken: this.turnstileToken
    }).pipe(
      timeout(this.requestTimeoutMs),
      catchError(err => {
        this.error = err?.error?.detail ?? 'Failed to send support request.';
        return throwError(() => err);
      }),
      finalize(() => {
        this.isSubmitting = false;
        this.resetTurnstile();
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: (response) => {
        this.name = '';
        this.email = '';
        this.topic = '';
        this.message = '';
        this.success = response?.message ?? 'Support request sent.';
        if (response?.warning) {
          this.error = response.warning;
        }
        if (this.success) {
          this.successTimeoutId = window.setTimeout(() => {
            this.success = null;
            this.cdr.markForCheck();
            this.successTimeoutId = null;
          }, 4000);
        }
        this.cdr.markForCheck();
      }
    });
  }

  private renderTurnstile() {
    const turnstile = (window as any).turnstile;
    const container = this.turnstileContainer?.nativeElement;
    if (!turnstile || !container) return;

    container.innerHTML = '';
    this.turnstileWidgetId = turnstile.render(container, {
      sitekey: this.turnstileSiteKey,
      callback: (token: string) => {
        this.turnstileToken = token;
        this.cdr.markForCheck();
      }
    });
  }

  private syncTurnstileToken() {
    if (!this.turnstileWidgetId) return;
    const turnstile = (window as any).turnstile;
    if (!turnstile?.getResponse) return;
    const token = turnstile.getResponse(this.turnstileWidgetId);
    if (token) {
      this.turnstileToken = token;
    }
  }

  private resetTurnstile() {
    if (!this.turnstileWidgetId) return;
    const turnstile = (window as any).turnstile;
    if (!turnstile?.reset) return;
    turnstile.reset(this.turnstileWidgetId);
    this.turnstileToken = '';
  }

  private loadTurnstileScript(): Promise<void> {
    if ((window as any).turnstile) {
      return Promise.resolve();
    }

    return new Promise((resolve, reject) => {
      const existing = document.querySelector<HTMLScriptElement>('script[data-turnstile="true"]');
      if (existing) {
        existing.addEventListener('load', () => resolve());
        existing.addEventListener('error', () => reject());
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit';
      script.async = true;
      script.defer = true;
      script.setAttribute('data-turnstile', 'true');
      script.onload = () => resolve();
      script.onerror = () => reject();
      document.head.appendChild(script);
    });
  }
}
