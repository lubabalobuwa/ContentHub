import { Component, ChangeDetectionStrategy, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { catchError, finalize, throwError, timeout } from 'rxjs';
import { environment } from '../../../../../environments/environments';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.page.html',
  styleUrl: './register.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterPage implements AfterViewInit, OnDestroy {
  error: string | null = null;
  isSubmitting = false;
  private readonly requestTimeoutMs = 15000;
  readonly turnstileSiteKey = environment.turnstileSiteKey;
  private turnstileWidgetId: string | null = null;

  form!: FormGroup;
  @ViewChild('turnstileContainer', { static: false }) turnstileContainer?: ElementRef<HTMLDivElement>;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      turnstileToken: ['', [Validators.required]]
    });
  }

  ngAfterViewInit() {
    if (!this.turnstileSiteKey) {
      return;
    }

    this.loadTurnstileScript()
      .then(() => this.renderTurnstile())
      .catch(() => {
        this.error = 'Captcha failed to load. Please refresh.';
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
    this.syncTurnstileToken();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, displayName, password, turnstileToken } = this.form.getRawValue();
    this.isSubmitting = true;

    this.auth.register(email, displayName, password, turnstileToken)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(err => {
          this.error = this.getErrorMessage(err, 'Registration failed. Try a different email.');
          return throwError(() => err);
        }),
        finalize(() => {
          this.isSubmitting = false;
          this.resetTurnstile();
        })
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/verify-email-sent'], { queryParams: { email } });
        }
      });
  }

  registerWithGoogle() {
    this.auth.externalLogin('google');
  }

  private getErrorMessage(error: any, fallback: string) {
    return error?.error?.detail
      ?? error?.error?.errors?.error?.[0]
      ?? fallback;
  }

  private renderTurnstile() {
    const turnstile = (window as any).turnstile;
    const container = this.turnstileContainer?.nativeElement;
    if (!turnstile || !container) return;

    container.innerHTML = '';
    this.turnstileWidgetId = turnstile.render(container, {
      sitekey: this.turnstileSiteKey,
      callback: (token: string) => {
        this.form.patchValue({ turnstileToken: token });
      }
    });
  }

  private syncTurnstileToken() {
    if (!this.turnstileWidgetId) return;
    const turnstile = (window as any).turnstile;
    if (!turnstile?.getResponse) return;
    const token = turnstile.getResponse(this.turnstileWidgetId);
    if (token) {
      this.form.patchValue({ turnstileToken: token });
    }
  }

  private resetTurnstile() {
    if (!this.turnstileWidgetId) return;
    const turnstile = (window as any).turnstile;
    if (!turnstile?.reset) return;
    turnstile.reset(this.turnstileWidgetId);
    this.form.patchValue({ turnstileToken: '' });
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
