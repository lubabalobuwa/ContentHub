import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.page.html',
  styleUrl: './forgot-password.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgotPasswordPage {
  form!: FormGroup;

  isSubmitting = false;
  message: string | null = null;
  error: string | null = null;

  constructor(private fb: FormBuilder, private auth: AuthService) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  submit() {
    this.error = null;
    this.message = null;
    if (this.form.invalid) return;

    this.isSubmitting = true;
    const email = this.form.value.email!;
    this.auth.forgotPassword(email).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.message = 'If an account exists, a reset email has been sent.';
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to request password reset.';
      }
    });
  }
}
