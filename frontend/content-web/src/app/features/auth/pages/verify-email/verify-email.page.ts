import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-verify-email-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './verify-email.page.html',
  styleUrl: './verify-email.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VerifyEmailPage {
  isSubmitting = true;
  success = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.isSubmitting = false;
      this.error = 'Verification token is missing.';
      this.cdr.markForCheck();
      return;
    }

    this.auth.verifyEmail(token).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.success = true;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Verification link is invalid or expired.';
        this.cdr.markForCheck();
      }
    });
  }
}
