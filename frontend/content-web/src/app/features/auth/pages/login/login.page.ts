import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { catchError, finalize, throwError, timeout } from 'rxjs';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPage {
  error: string | null = null;
  isSubmitting = false;
  isResending = false;
  showResend = false;
  resendMessage: string | null = null;
  private readonly requestTimeoutMs = 15000;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  submit() {
    this.error = null;
    this.resendMessage = null;
    this.showResend = false;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();
    this.isSubmitting = true;

    this.auth.login(email, password)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(err => {
          this.error = this.getErrorMessage(err, 'Invalid email or password.');
          this.cdr.markForCheck();
          return throwError(() => err);
        }),
        finalize(() => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.router.navigateByUrl('/');
        }
      });
  }

  loginWithGoogle() {
    this.auth.externalLogin('google');
  }

  resendVerification() {
    const email = this.form.get('email')?.value;
    if (!email) {
      this.error = 'Enter your email first.';
      this.cdr.markForCheck();
      return;
    }

    this.isResending = true;
    this.resendMessage = null;
    this.auth.resendVerification(email)
      .pipe(finalize(() => {
        this.isResending = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: () => {
          this.resendMessage = 'Verification email sent. Please check your inbox.';
        },
        error: () => {
          this.error = 'Failed to resend verification email.';
        }
      });
  }

  private getErrorMessage(error: any, fallback: string) {
    const detail = error?.error?.detail as string | undefined;
    if (error?.status === 403) {
      if (detail?.toLowerCase().includes('disabled')) {
        return 'User disabled. Contact support.';
      }
      if (detail?.toLowerCase().includes('not verified')) {
        this.showResend = true;
        return 'Email not verified. Check your inbox.';
      }
      return 'Access denied.';
    }

    return detail
      ?? error?.error?.errors?.error?.[0]
      ?? fallback;
  }
}
