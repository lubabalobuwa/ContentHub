import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { UserProfile } from '../../../../core/models/user-profile.model';

@Component({
  selector: 'app-account-settings-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './account-settings.page.html',
  styleUrl: './account-settings.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountSettingsPage {
  profile$;

  constructor(private auth: AuthService) {
    this.profile$ = this.auth.me().pipe(
      catchError(() => of(null as UserProfile | null))
    );
  }
}
