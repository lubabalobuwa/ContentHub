import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-verify-email-sent-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email-sent.page.html',
  styleUrl: './verify-email-sent.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VerifyEmailSentPage {
  email = '';
  isSubmitting = false;
  message: string | null = null;
  error: string | null = null;

  constructor(private route: ActivatedRoute, private auth: AuthService) {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
  }

  resend() {
    this.error = null;
    this.message = null;
    if (!this.email) {
      this.error = 'Email is missing.';
      return;
    }

    this.isSubmitting = true;
    this.auth.resendVerification(this.email).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.message = 'Verification email sent. Please check your inbox.';
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to resend verification email.';
      }
    });
  }
}
