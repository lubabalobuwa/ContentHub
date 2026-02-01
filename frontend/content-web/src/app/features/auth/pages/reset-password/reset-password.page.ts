import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.page.html',
  styleUrl: './reset-password.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordPage {
  form!: FormGroup;

  isSubmitting = false;
  message: string | null = null;
  error: string | null = null;
  private token: string | null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private auth: AuthService
  ) {
    this.form = this.fb.nonNullable.group({
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
    this.token = this.route.snapshot.queryParamMap.get('token');
  }

  submit() {
    this.error = null;
    this.message = null;
    if (this.form.invalid || !this.token) {
      this.error = 'Reset token is missing or invalid.';
      return;
    }

    this.isSubmitting = true;
    const password = this.form.value.password!;
    this.auth.resetPassword(this.token, password).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.message = 'Password reset successful. You can now log in.';
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Reset link is invalid or expired.';
      }
    });
  }
}
