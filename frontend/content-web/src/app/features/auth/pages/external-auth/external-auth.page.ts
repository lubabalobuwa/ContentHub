import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { catchError, timeout } from 'rxjs';

@Component({
  selector: 'app-external-auth-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './external-auth.page.html',
  styleUrl: './external-auth.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExternalAuthPage {
  error: string | null = null;
  private readonly requestTimeoutMs = 15000;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    const code = this.route.snapshot.queryParamMap.get('code') ?? '';
    if (!code) {
      this.error = 'Missing authentication code.';
      return;
    }

    this.auth.exchangeExternalLogin(code)
      .pipe(
        timeout(this.requestTimeoutMs),
        catchError(err => {
          if (err?.status === 429) {
            this.error = 'Too many attempts. Please wait a minute and try again.';
          } else if (err?.name === 'TimeoutError') {
            this.error = 'Sign-in timed out. Please try again.';
          } else {
            this.error = 'External sign-in failed. Please try again.';
          }

          this.cdr.markForCheck();
          throw err;
        })
      )
      .subscribe({
        next: () => this.router.navigateByUrl('/', { replaceUrl: true })
      });
  }
}
