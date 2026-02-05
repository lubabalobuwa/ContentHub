import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { catchError, finalize, throwError, timeout } from 'rxjs';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.page.html',
  styleUrl: './register.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterPage {
  error: string | null = null;
  isSubmitting = false;
  private readonly requestTimeoutMs = 15000;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  submit() {
    this.error = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, displayName, password } = this.form.getRawValue();
    this.isSubmitting = true;

    this.auth.register(email, displayName, password)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(err => {
          this.error = this.getErrorMessage(err, 'Registration failed. Try a different email.');
          return throwError(() => err);
        }),
        finalize(() => {
          this.isSubmitting = false;
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
}
